using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Clarius.Edu.Graph;

namespace Clarius.Edu.CLI
{
    internal class ListGroupsCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresUser => false;

        internal ListGroupsCommand(string[] args) : base(args)
        {
        }

        
        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
                                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                                .CreateLogger();

            var groups = Client.GetGroups(base.Type, base.Level, base.Grade, base.Division);

            var groupNames = new List<string>();

            int count = 0;
            foreach (var group in groups)
            {
                if (base.Filter != null && !group.DisplayName.Contains(base.Filter, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                groupNames.Add($"{group.DisplayName}, {Client.RemoveDomainPart(group.MailNickname)}");
                count++;
            }

            groupNames.Sort();
            groupNames.ForEach(Log.Logger.Information);

            Log.Logger.Information("");
            Log.Logger.Information($"{count} groups found");

        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--listgroups", "-lg" };
        }

        public override string Name => "List Groups";
        public override string Description => "List groups filtered by metadata, i.e. edu.exe -lg /type:class";

        public override CommandTarget AppliesTo => CommandTarget.Group;


    }
}
