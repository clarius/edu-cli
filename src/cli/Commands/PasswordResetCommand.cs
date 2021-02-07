using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Clarius.Edu.CLI
{
    internal class PasswordResetCommand : CommandBase
    {
        public override bool UseCache => false;
        public override bool RequiresUser => true;

        internal PasswordResetCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            var usr = Client.GetUser(User);

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            Log.Logger.Information("");
            Log.Logger.Information($"Username:              {User.UserPrincipalName}");
            Log.Logger.Information($"DisplayName:           {User.DisplayName}");
            Log.Logger.Information($"Last password change was on: {User.LastPasswordChangeDateTime.Value.LocalDateTime}");
            Log.Logger.Information("");

            var newPassword = base.Password;

            if (newPassword == null)
            {
                Log.Logger.Error("Please specify a new password using /password:YourNewPassword, i.e. edu.exe -rp username /password:Office123");
                return;
            }

            await Client.Graph.Users[base.User.Id].Request().UpdateAsync(new User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = base.Password,
                    ForceChangePasswordNextSignIn = true
                },
            });

            Log.Logger.Information($"Changed {base.User.UserPrincipalName} password to {base.Password}");
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--resetpwd", "-rp" };
        }

        // NOTE: this flag is required so the app will ask for users creds as apps don't have permission to do password resets,
        // so look at a browsing window opening and log in there
        public override bool UseAppPermissions => false;    

        public override string Name => "Reset Password";
        public override string Description => "Resets the password of a given user, i.e. edu.exe username -rp /p:Office123";
    }
}
