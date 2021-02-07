using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Clarius.Edu.CLI
{
    internal class ListUsersCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresUser => false;

        internal ListUsersCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
                                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                                .CreateLogger();

            var users = Client.GetUsers(Client.GetUsers(), base.Grade, base.Division, base.EnglishLevel, base.Level, base.Type);

                int count = 0;
            foreach (var user in users)
            {
                if (RawFormat)
                {
                    Log.Logger.Information($"{Client.RemoveDomainPart(user.UserPrincipalName)}");
                }
                else
                {
                    Log.Logger.Information($"{user.DisplayName}");
                }
                count++;
            }

            if (!RawFormat)
            {
                Log.Logger.Information("");
                Log.Logger.Information($"Total {count} items found");
            }

        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--listusers", "-lu" };
        }

        public override string Name => "List Users";
        public override string Description => "List users filtered by metadata, i.e. edu.exe -lu /type:Estudiante /level:Inicial";


    }
}
