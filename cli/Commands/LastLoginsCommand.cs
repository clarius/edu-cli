using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Clarius.Edu.CLI
{
    internal class LastLoginsCommand : CommandBase
    {
        public override bool UseCache => false;
        public override bool RequiresUser => true;

        internal LastLoginsCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            var usr = Client.GetUser(User);

           var lastLogins = await Client.Graph.AuditLogs.SignIns.Request().Filter($"userId eq '{User.Id}'").OrderBy("createdDateTime desc").Top(50).GetAsync();

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            Log.Logger.Information("");
            Log.Logger.Information($"Username:              {User.UserPrincipalName}");
            Log.Logger.Information($"DisplayName:           {User.DisplayName}");
            Log.Logger.Information("");

            for (int c=lastLogins.Count-1; c>0; c--)
            {
                var ll = lastLogins[c];

                if (Verbose)
                {
                    Log.Logger.Information($"When:              {ll.CreatedDateTime.Value.LocalDateTime}");
                    Log.Logger.Information($"Client:            {ll.ClientAppUsed}");
                    Log.Logger.Information($"AppDisplayName:    {ll.AppDisplayName}");
                    Log.Logger.Information($"Browser:           {ll.DeviceDetail.Browser}");
#if USE_BETA
                    Log.Logger.Information($"BrowserId:         {ll.DeviceDetail.BrowserId}");
#endif
                    Log.Logger.Information($"DeviceId:          {ll.DeviceDetail.DeviceId}");
                    Log.Logger.Information($"DisplayName:       {ll.DeviceDetail.DisplayName}");
                    Log.Logger.Information($"OperatingSystem:   {ll.DeviceDetail.OperatingSystem}");
                    Log.Logger.Information("---------------------------------------------------");
                }
                else
                {
                    Log.Logger.Information($"{ll.CreatedDateTime.Value.LocalDateTime}");
                }

            }
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--lastlogins", "-ll" };
        }

        public override string Name => "Last Logins";
        public override string Description => "List last login activity of a given user, i.e. edu.exe -ll juan.perez";
    }
}
