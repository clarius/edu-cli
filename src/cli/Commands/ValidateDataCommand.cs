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
    internal class ValidateDataCommand : CommandBase
    {
        // TODO: we could set this to 'false' as soon as we add a way to get a group by alias (see Client::GetGroupFromAlias)
        public override bool UseCache => true;

        public override string Name => "Checks integrity of user ";
        public override string Description => "Validates metadata value for all users.";
        internal ValidateDataCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            int errorCount = 0;
            int noProfile = 0;

            foreach (var u in Client.GetUsers())
            {
                var usr = Client.GetUser(u);


                if (usr.InternalProfile != null)
                {
                    //if (usr.Type != Clarius.Edu.Graph.Constants.USER_TYPE_STUDENT)
                    //    continue;

                    if (!Client.ValidateGrade(usr.Grade))
                    {
    //                    Log.Logger.Error($"User {u.UserPrincipalName} has an invalid Grade: {usr.Grade}");
                        errorCount++;
                    }
                    if (!Client.ValidateDivision(usr.Division))
                    {
      //                  Log.Logger.Error($"User {u.UserPrincipalName} has an invalid Division: {usr.Division}");
                        errorCount++;
                    }
                    if (!Client.ValidateLevel(usr.Level))
                    {
        //                Log.Logger.Error($"User {u.UserPrincipalName} has an invalid Level: {usr.Level}");
                        errorCount++;
                    }

                    //Log.Logger.Information("--- Profile -=-");
                    //Log.Logger.Information($"Id:                    {usr.Id}");
                    //Log.Logger.Information($"Level:                 {usr.Level}");
                    //Log.Logger.Information($"Type:                  {usr.Type}");
                    //Log.Logger.Information($"Grade:                 {usr.Grade}");
                    //Log.Logger.Information($"Division:              {usr.Division}");
                    //Log.Logger.Information($"EnglishLevel:          {usr.EnglishLevel}");
                    //Log.Logger.Information($"Year:                  {usr.Year}");
                }
                else
                {
                   Log.Logger.Warning($"{u.UserPrincipalName}");
                    noProfile++;
                }
            }


            //if (errorCount > 0)
            //    Log.Logger.Information($"Total errors: {errorCount}");

            if (noProfile > 0)
                Log.Logger.Information($"Total users with no profile (all types): {noProfile}");

        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--validatedata", "-vd" };
        }

    }
}
