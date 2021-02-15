using Clarius.Edu.Graph;
using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Clarius.Edu.CLI
{
    internal abstract class CommandBase : ICommand
    {
        private const string DEBUG_PARAMETER = "/debug";
        private const string DISABLED_PARAMETER = "/disabled";
        private const string VALUE_PARAMETER = "/value:";

        // TODO: the following two should belong to ListCalendar cmd
        internal string HasMeeting { get; private set; }
        internal string Organizer { get; private set; }

        internal string From { get; private set; }
        internal string Folder { get; private set; }

        internal string Password { get; private set; }
        internal bool Disabled { get; private set; }
        internal int? Top { get; private set; }
        internal string Output { get; private set; }
        internal string Config { get; private set; }
        internal bool Debug { get; private set; }
        internal string Filter { get; private set; }
        internal string Id { get; private set; }
        internal string EnglishLevel { get; private set; }
        internal string Level { get; private set; }
        internal string Type { get; private set; }
        internal string Division { get; private set; }
        internal string Grade { get; private set; }
        internal string Year { get; private set; }
        internal User User { get; private set; }
        internal Microsoft.Graph.Group Group { get; private set; }
        internal string[] AliasesList { get; private set; }
        internal string AliasesFile { get; private set; }
        internal string OutputFile { get; private set; }
        internal bool Verbose { get; private set; }
        internal string[] Arguments { get; private set; }
        internal Client Client { get; private set; }
        internal bool RawFormat { get; private set; }
        internal bool ExcelFormat { get; private set; }
        internal bool Value { get; private set; }

        internal CommandBase(string[] args)
        {
            this.Arguments = args;
        }
        abstract public List<string> GetSupportedCommands();

        virtual public async Task<bool> Run(string[] args, bool verbose)
        {
            Debug = ReadBoolValue(DEBUG_PARAMETER);
            Disabled = ReadBoolValue(DISABLED_PARAMETER);
            var str = GetParameterValue(VALUE_PARAMETER);
            if (str == null && RequiresValue)
            {
                Log.Logger.Error($"You must provide a /value parameter for this command to work");
                return false;
            }
            bool flag = false;
            if (str != null && !bool.TryParse(str, out flag))
            {
                Log.Logger.Error($"Invalid value {str} for /value: parameter. You must specify either true or false.");
                return false;
            }
            Value = flag;

            Output = GetParameterValue("/output:");
            if (string.IsNullOrEmpty(Output))
            {
                RawFormat = GetParameterValue("/raw") != null;
            }
            else
            {
                RawFormat = string.Equals(Output, "raw", StringComparison.InvariantCultureIgnoreCase);
                ExcelFormat = string.Equals(Output, "excel", StringComparison.InvariantCultureIgnoreCase);
            }

            Config = GetParameterValue("/config:") ?? "settings.config";

            Client = new Client(Config, RawFormat);
            this.Arguments = args;
            this.Verbose = verbose;

            await Client.Connect(UseAppPermissions, UseCache, RetrieveEducationUses);

            var canContinue = await this.ResolveCommonParameters();
            if (!canContinue)
            {
                return false;
            }

            if (!RawFormat)
            {
                Log.Logger.Information($"Starting {this.GetType().Name}...");
            }

            await RunInternal();

            return true;
        }

        async virtual public Task RunInternal()
        {
            throw new NotImplementedException();
        }

        bool ICommand.CanHandle(string commandName)
        {
            return GetSupportedCommands().Contains(commandName);
        }

        internal bool ReadBoolValue(string parameterName)
        {
            foreach (var str in Arguments)
            {
                if (str.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
        internal string GetParameterValue(string param)
        {
            foreach (var str in Arguments)
            {
                if (str.ToLowerInvariant().StartsWith(param.ToLowerInvariant()))
                {
                    return str.Substring(param.Length);
                }
            }

            return null;
        }

        abstract public bool UseCache { get; }

        virtual public bool RetrieveEducationUses
        {
            get { return false; }
        }

        virtual public bool UseAppPermissions
        {
            get { return true; }
        }

        public virtual CommandTarget AppliesTo => CommandTarget.Other;

        public virtual bool RequiresUser => false;
        public virtual bool RequiresGroup => false;
        public virtual bool RequiresLevel => false;
        public virtual bool RequiresValue => false;

        public virtual string Name => "Unnamed";

        public virtual string Description => "Empty description";

        async Task<Microsoft.Graph.Group> ResolveGroup()
        {
            if (Arguments.Length > 1)
            {
                var groupalias = Arguments[1];

                return await Client.GetGroupFromAlias(groupalias);
            }

            return null;
        }

        async Task<User> ResolveUser()
        {
            if (Arguments.Length > 1)
            {
                var usernameOrFilename = Arguments[1];
                return await Client.GetUserFromUserPrincipalName(usernameOrFilename);
            }

            return null;
        }
        string[] ResolveAliasList()
        {
            AliasesFile = GetParameterValue("/file:");

            if (string.IsNullOrEmpty(AliasesFile))
                return null;

            try
            {
                AliasesList = System.IO.File.ReadAllLines(AliasesFile);
            }
            catch (FileNotFoundException)
            {
                // a file not found is expected, as this method can be called without knowing if the user has specified a filename or an username
                return null;
            }
            catch (Exception ex)
            {
                // if the error is something else, we want to log it, just in case (this may be a valid filename that we can't access for some reason)
                Log.Logger.Warning($"Error: {ex.Message}");
                return null;
            }

            return AliasesList;
        }

        async internal Task<bool> ResolveCommonParameters()
        {
            string[] aliasList = ResolveAliasList();
            if (aliasList == null)
            {
                try
                {
                    User = await ResolveUser();
                }
                catch (Exception ex)
                {
                    // Log.Logger.Warning(ex.Message);
                }
            }

            if (aliasList == null && User == null && RequiresUser)
            {
                Log.Logger.Error("You must specify either an username or a file with an user list for this command to work");
                return false;
            }

            // if no user or user list specified, try to retrieve a group
            if (aliasList == null && User == null)
            {
                try
                {
                    Group = await ResolveGroup();
                }
                catch (Exception ex)
                {
                    // Log.Logger.Warning(ex.Message);
                }

                if (Group == null && aliasList == null && RequiresGroup)
                {
                    Log.Logger.Error("You must specify a group alias or file with a group alias list for this command to work");
                    return false;
                }
            }

            var str = GetParameterValue("/top:");
            if (str != null)
            {
                int top;
                if (!int.TryParse(str, out top))
                {
                    Log.Logger.Error($"Value for parameter /top is invalid: {str}");
                    return false;
                }
                Top = top;
            }

            Password = GetParameterValue("/password:");
            HasMeeting = GetParameterValue("/hasmeeting:");
            Filter = GetParameterValue("/filter:");
            Level = GetParameterValue("/level:");

            if (string.IsNullOrEmpty(Level) && RequiresLevel)
            {
                Log.Logger.Error($"/level argument is required");
                return false;
            }
            if (!string.IsNullOrEmpty(Level) && !Client.ValidateLevel(Level))
            {
                Log.Logger.Error($"Invalid value '{Level}' specified for /level argument");
                return false;
            }
            Organizer = GetParameterValue("/organizer:");
            From = GetParameterValue("/from:");
            Folder = GetParameterValue("/folder:");
            OutputFile = GetParameterValue("/outputfile:");
            Year = GetParameterValue("/year:");

            if (Level != null && !Client.ValidateLevel(Level))
            {
                Log.Logger.Error($"Value for parameter /level is invalid: {Level}");
                return false;
            }
            Type = GetParameterValue("/type:");


            if (AppliesTo == CommandTarget.User)
            {
                if (Type != null && !Client.ValidateUserType(Type))
                {
                    Log.Logger.Error($"Value for parameter /type is invalid: {Type}");
                    return false;
                }
            }
            else if (AppliesTo == CommandTarget.Group)
            {
                if (Type != null && !Client.ValidateGroupType(Type))
                {
                    Log.Logger.Error($"Value for parameter /type is invalid: {Type}");
                    return false;
                }
            }

            Division = GetParameterValue("/division:");
            if (Division != null && !Client.ValidateDivision(Division))
            {
                Log.Logger.Error($"Value for parameter /division is invalid: {Division}");
                return false;
            }
            Grade = GetParameterValue("/grade:");
            if (Grade != null && !Client.ValidateGrade(Grade))
            {
                Log.Logger.Error($"Value for parameter /grade is invalid: {Grade}");
                return false;
            }
            EnglishLevel = GetParameterValue("/englishlevel:");
            if (EnglishLevel != null && !Client.ValidateEnglishLevel(EnglishLevel))
            {
                Log.Logger.Error($"Value for parameter /englishlevel is invalid: {EnglishLevel}");
                return false;
            }
            Id = GetParameterValue("/id:");

            if (Debug)
            {
                Log.Logger.Information($"/filter = {Filter}");
                Log.Logger.Information($"/level = {Level}");
                Log.Logger.Information($"/type = {Type}");
                Log.Logger.Information($"/division = {Division}");
                Log.Logger.Information($"/grade = {Grade}");
                Log.Logger.Information($"/englishlevel = {EnglishLevel}");
                Log.Logger.Information($"/id = {Id}");
            }

            return true;
        }
    }
}
