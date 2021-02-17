using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Globalization;
using Clarius.Edu.Graph;
using CsvHelper.Configuration;

namespace Clarius.Edu.CLI
{
    internal class CreateUsersCommand : CommandBase
    {
        internal CreateUsersCommand(string[] args) : base(args) { }

        async public override Task RunInternal()
        {
            if (AliasesList == null || AliasesList.Length == 0)
            {
                Log.Logger.Error("Must specify a cvs file using the /file: parameter");
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            // do a first pass to check for data validation
            int errorCount = 0;
            int recordCount = 0;
            using (var reader = new StreamReader(base.AliasesFile))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<ImportedUser>();

                foreach (var csvUser in records)
                {
                    if (!Client.ValidateUserType(csvUser.Type))
                    {
                        Log.Logger.Warning($"User '{csvUser.Username} is of unknown Type '{csvUser.Type}', skipping it...");
                        errorCount++;
                    }

                    if (!Client.ValidateLevel(csvUser.Level))
                    {
                        Log.Logger.Warning($"User '{csvUser.Username} has unknown Level '{csvUser.Level}', skipping it...");
                        errorCount++;
                    }

                    if (!Client.ValidateGrade(csvUser.Grade))
                    {
                        Log.Logger.Warning($"User '{csvUser.Username} has unknown Grade '{csvUser.Grade}', skipping it...");
                        errorCount++;
                    }

                    if (!Client.ValidateDivision(csvUser.Division))
                    {
                        Log.Logger.Warning($"User '{csvUser.Username}' has unknown Division '{csvUser.Division}', skipping it...");
                        errorCount++;
                    }

                    if (Client.UserAlreadyExists(csvUser.Username))
                    {
                        Log.Logger.Warning($"User {csvUser.Username} already exists online. Skipping it...");
                        errorCount++;
                    }

                    recordCount++;
                }
            }

            if (errorCount > 0)
            {
                Log.Logger.Error("Please fix the errors above and run this command again");
                //return;
            }

            recordCount = 0;
            // second pass to actually create the users now
            using (var reader = new StreamReader(base.AliasesFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<ImportedUser>();

                foreach (var csvUser in records)
                {
                    await Client.CreateUser(csvUser.Username, csvUser.Password, csvUser.FirstName.Trim(), csvUser.LastName.Trim(), csvUser.Type, csvUser.Level, csvUser.Grade, csvUser.Division, "0", DateTime.Now.Year.ToString(), "");
                    Log.Logger.Information($"User created: {csvUser.Username}");
                    recordCount++;
                }
            }

            Log.Logger.Information($"Processed {recordCount} records");
            if (errorCount > 0)
            {
                Log.Logger.Error($"Found {errorCount} errors");
                return;
            }
            else
            {
                Log.Logger.Information("No errors found!");
            }
        }


        //try
        //{
        //    var vuser = Client.GetUser(await Client.GetUserFromUserPrincipalName(csvUser.Username));
        //    var customMetadata = new Dictionary<string, object>
        //    {
        //        { Constants.USERGRADE, vuser.Grade },
        //        { Constants.USERLEVEL, csvUser.Level },
        //        { Constants.USERDIVISION, csvUser.Division },
        //        { Constants.USERTYPE, Clarius.Edu.Graph.Constants.USER_TYPE_STUDENT },
        //        { Constants.USERENGLISHLEVEL, "1" },
        //    };
        //    try
        //    {
        //        await vuser.UpdateExtension(Constants.EXTENSION_ID, customMetadata);
        //        //Log.Logger.Information("User updated!");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Warning($"{ex.Message}");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Log.Logger.Error(ex.Message);
        //}

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--CreateUsers", "-cu" };
        }

        public override bool UseCache => true;

        public override string Name => "Create Users";
        public override string Description => "Creates one or more users from a .csv file";
    }
}
