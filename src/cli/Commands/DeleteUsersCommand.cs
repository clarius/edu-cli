using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Globalization;
using Clarius.Edu.Graph;
using CsvHelper.Configuration;
using Microsoft.Graph;

namespace Clarius.Edu.CLI
{
    internal class DeleteUsersCommand : CommandBase
    {
        internal DeleteUsersCommand(string[] args) : base(args) { }
        public override bool RequiresUser => true;
        public override bool UseAppPermissions => false;

        async public override Task RunInternal()
        {
            if (base.User != null)
            {
                var userData = $"{base.User.DisplayName} ({base.User.UserPrincipalName})";
                await Client.Graph.Users[base.User.Id].Request().DeleteAsync();
                Log.Logger.Information($"User {userData} has been deleted");
                return;
            }

            int errorCount = 0;
            List<User> userList = new List<User>();

            // quick check that all specified users do exist
            foreach (var line in base.AliasesList)
            {
                var user = await Client.GetUserFromUserPrincipalName(line);
                if (user == null)
                {
                    Log.Logger.Warning($"{line} does not exists");
                    errorCount++;
                }
                userList.Add(user);
            }

            if (errorCount > 0)
            {
                Log.Logger.Error($"Total errors found: {errorCount}. Please fix errors and re-run this command.");
                return;
            }

            int recordCount = 0;
            errorCount = 0;

            // second pass to actually delete the users now
            foreach (var user in userList)
            {
                try
                {
                    await Client.Graph.Users[user.Id].Request().DeleteAsync();
                    recordCount++;
                    Log.Logger.Information($"User {user.UserPrincipalName} deleted");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.Message);
                    errorCount++;
                }
            }

            if (errorCount > 0)
            {
                Log.Logger.Error($"Total errors found: {errorCount}. Some deletions may have failed, please review log file.");
                return;
            }

            Log.Logger.Information($"Processed {recordCount} records");
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--Delete Users", "-du" };
        }

        public override bool UseCache => true;

        public override string Name => "Delete Users";
        public override string Description => "Deletes one or more users";
    }
}
