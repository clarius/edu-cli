using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.Design;

namespace Clarius.Edu.CLI
{
    internal class WhoIsCommand : CommandBase
    {
        // TODO: we could set this to 'false' as soon as we add a way to get a group by alias (see Client::GetGroupFromAlias)
        public override bool UseCache => true;

        public override string Name => "WhoIs";
        public override string Description => "List basic info about a specified user or group, including internal profile metadata.";
        internal WhoIsCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            if (base.StdIn.Count > 0)
            {
                Log.Logger.Information($"HAS INPUT!!!");
            }

            if (User != null)
            {
                var usr = Client.GetUser(User);

                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                .CreateLogger();

                Log.Logger.Information("");
                Log.Logger.Information($"Username:              {User.UserPrincipalName}");
                Log.Logger.Information($"DisplayName:           {User.DisplayName}");
                Log.Logger.Information($"Id:                    {User.Id}");
                Log.Logger.Information($"LastPasswordChange:    {User.LastPasswordChangeDateTime.Value.ToLocalTime()}");
                Log.Logger.Information($"PreferredLanguage:     {User.PreferredLanguage}");

                if (usr.InternalProfile != null)
                {
                    Log.Logger.Information("--- Profile -=-");
                    Log.Logger.Information($"Id:                    {usr.Id}");
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
            else if (base.Group != null)
            {
                var grp = Client.GetGroup(base.Group);

                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                .CreateLogger();

                Log.Logger.Information("");
                Log.Logger.Information($"DisplayName:           {Group.DisplayName}");
                Log.Logger.Information($"Id:                    {Group.Id}");
                Log.Logger.Information($"Nickname               {Group.MailNickname}");

                if (grp.InternalProfile != null)
                {
                    Log.Logger.Information("--- Profile ---");
                    Log.Logger.Information($"Id:                    {grp.Id}");
                    Log.Logger.Information($"Level:                 {grp.Level}");
                    Log.Logger.Information($"Type:                  {grp.Type}");
                    Log.Logger.Information($"Grade:                 {grp.Grade}");
                    Log.Logger.Information($"Division:              {grp.Division}");
                    Log.Logger.Information($"Year:                  {grp.Year}");
                }
                else
                {
                    Log.Logger.Warning($"Group doesn't have an internal profile!");
                }
            }
            else
            {
                Log.Logger.Error("Either an user or group must be specified");
            }
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--whois", "-w" };
        }

    }
}
