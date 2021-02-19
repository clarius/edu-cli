using Serilog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Clarius.Edu.CLI
{
    internal class GroupMembersCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresGroup => true;

        internal GroupMembersCommand(string[] args) : base(args)
        {
        }


        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
                                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                                .CreateLogger();

            var group = base.Group;

            // we need to check for this as we don't support specifying a file with group aliases just yet
            if (group == null)
            {
                Log.Logger.Error($"You must specify a single group for this command to work");
                return;
            }

            if (!RawFormat)
            {
                Log.Logger.Information($"{group.DisplayName}");
                Log.Logger.Information("");
            }

            var members = await Client.Graph.Groups[group.Id].Members.Request().GetAsync();
            var list = new List<string>();

            foreach (var member in members)
            {
                var usr = Client.GetUserFromId(new Guid(member.Id));
                if (RawFormat)
                {
                    list.Add($"{Client.RemoveDomainPart(usr.UserPrincipalName)}");
                }
                else
                {
                    list.Add($"{usr.DisplayName}");
                }
            }

            list.Sort();
            list.ForEach(Log.Logger.Information);

            if (!RawFormat)
            {
                Log.Logger.Information($"Total items: {members.Count}");
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
