using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clarius.Edu.CLI
{
    internal class GroupMembersCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresUser => false;

        internal GroupMembersCommand(string[] args) : base(args)
        {
        }


        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
                                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                                .CreateLogger();

            var group = Client.GetGroupFromEmail(Arguments[1]);

            if (group == null)
            {
                Log.Logger.Error($"Group '{Arguments[1]}' not found");
                return;
            }

            if (!RawFormat)
            {
                Log.Logger.Information($"{group.DisplayName}");
                Log.Logger.Information("");
            }

            var members = await Client.Graph.Groups[group.Id].Members.Request().GetAsync();

            foreach (var member in members)
            {
                var usr = Client.GetUserFromId(new Guid(member.Id));
                if (RawFormat)
                {
                    Log.Logger.Information($"{Client.RemoveDomainPart(usr.UserPrincipalName)}");
                }
                else
                {
                    Log.Logger.Information($"{usr.DisplayName}");
                }
            }

        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--groupmembers", "-gm" };
        }

        public override string Name => "Group Members";
        public override string Description => "List all members of a given group, i.e. edu.exe -gm docentesinicial";


    }
}
