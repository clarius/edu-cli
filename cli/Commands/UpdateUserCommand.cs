using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Clarius.Edu.Graph;
using Constants = Clarius.Edu.Graph.Constants;

namespace Clarius.Edu.CLI
{
    internal class UpdateUserCommand : CommandBase
    {
        public override bool UseCache => false;

        internal UpdateUserCommand(string[] args) : base (args)
        {
        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var usersToProcess = new List<User>();
            if (AliasesList != null)
            {
                foreach (var userLine in AliasesList)
                {
                    var u = await Client.GetUserFromUserPrincipalName(userLine);
                    if (u != null)
                    {
                        usersToProcess.Add(u);
                    }
                    else
                    {
                        Log.Logger.Warning($"Cannot find user {userLine}");
                    }
                }
            }
            else
            {
                if (User != null)
                    usersToProcess.Add(User);
            }

            if (usersToProcess.Count > 0)
            {
                await ProcessUsers(usersToProcess);
                return;
            }

            Log.Logger.Error("You must specify either an user alias or user alias list (text file, using /file:) for this command to work");
        }

        async Task ProcessUsers(List<User> usersToProcess)
        {
            var newLevel = base.GetParameterValue("/level:");
            var newType = base.GetParameterValue("/type:");
            var newDivision = base.GetParameterValue("/division:");
            var newGrade = base.GetParameterValue("/grade:");
            var newEnglishLevel = base.GetParameterValue("/englishlevel:");
            var newYear = base.GetParameterValue("/year:");

            Log.Logger.Information($"Going to process {usersToProcess.Count} users...");

            var printDetails = usersToProcess.Count == 1;

            foreach (var user in usersToProcess)
            {
                if (printDetails)
                {
                    Log.Logger.Information("");
                    Log.Logger.Information($"Username:              {user.UserPrincipalName}");
                    Log.Logger.Information($"DisplayName:           {user.DisplayName}");
                    Log.Logger.Information($"LastPasswordChange:    {user.LastPasswordChangeDateTime}");
                    Log.Logger.Information($"PreferredLanguage:     {user.PreferredLanguage}");
                }
                else
                {
                    Log.Logger.Information($"{user.UserPrincipalName}");
                }

                var usr = Client.GetUser(user);

                if (printDetails)
                {
                    PrintProfile(usr);
                }

                if (usr.InternalProfile == null)
                {
                    var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_USERGRADE, newGrade ?? ""},
                            { Constants.PROFILE_USERDIVISION, newDivision ?? ""},
                            { Constants.PROFILE_USERLEVEL, newLevel ?? "" },
                            { Constants.PROFILE_USERTYPE, newType ?? "" },
                            { Constants.PROFILE_USERENGLISHLEVEL, newEnglishLevel ?? "" },
                            { Constants.PROFILE_USERYEAR, newYear ?? "" },
                        };
                    await usr.CreateExtension(Constants.PROFILE_USEREXTENSION_ID, customMetadata);
                    Log.Logger.Information("User extension created!");
                }
                else
                {
                    var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_USERGRADE, newGrade ?? usr.Grade },
                            { Constants.PROFILE_USERDIVISION, newDivision ?? usr.Division },
                            { Constants.PROFILE_USERLEVEL, newLevel ?? usr.Level },
                            { Constants.PROFILE_USERTYPE, newType ?? usr.Type },
                            { Constants.PROFILE_USERENGLISHLEVEL, newEnglishLevel ?? usr.EnglishLevel },
                            { Constants.PROFILE_USERYEAR, newYear ?? usr.Year },
                        };
                    try
                    {
                        await usr.UpdateExtension(Constants.PROFILE_USEREXTENSION_ID, customMetadata);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Warning($"Failed to update extension: {ex.Message}");
                    }
                }
            }
        }

        static void PrintProfile(VUser usr)
        {
            if (usr.InternalProfile != null)
            {
                Log.Logger.Information($"Level:                 {usr.Level}");
                Log.Logger.Information($"Type:                  {usr.Type}");
                Log.Logger.Information($"Grade:                 {usr.Grade}");
                Log.Logger.Information($"Division:              {usr.Division}");
                Log.Logger.Information($"EnglishLevel:          {usr.EnglishLevel}");
                Log.Logger.Information($"Year:                  {usr.Year}");
            }
            else
            {
                Log.Logger.Warning($"User doesn't have an internal profile!");
            }
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--updateuser", "-uu" };
        }

        public override string Name => "Update User";
        public override string Description => "Updates the metadata of a given user";
    }
}
