using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Clarius.Edu.Graph;
using Constants = Clarius.Edu.Graph.Constants;

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

            //var foo = await Client.GetGroupFromAlias("ijme");
            //var customMetadata = new Dictionary<string, object>
            //            {
            //                { Constants.PROFILE_GROUPGRADE, ""},
            //                { Constants.PROFILE_GROUPDIVISION, ""},
            //                { Constants.PROFILE_GROUPLEVEL, "Primaria"},
            //                { Constants.PROFILE_GROUPTYPE, Constants.GROUP_TYPE_SUPPORT },
            //                { Constants.PROFILE_GROUPYEAR, "2020" },
            //                { Constants.PROFILE_GROUPID, "IJME" },
            //            };

            //var vf = Client.GetGroup(foo);
            //await vf.UpdateExtension(Constants.PROFILE_GROUPEXTENSION_ID, customMetadata);

            
            var groups = Client.GetGroups(base.Type, base.Level, base.Grade, base.Division, base.Year);

            var groupNames = new List<string>();

            int count = 0;
            foreach (var group in groups)
            {
                var grp = Client.GetGroup(group);
                if (!string.IsNullOrEmpty(base.Filter) && !group.DisplayName.Contains(base.Filter, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(base.Year) && !string.Equals(grp.Year, base.Year, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (!base.RawFormat)
                {
                    groupNames.Add($"{group.DisplayName}, {Client.RemoveDomainPart(group.MailNickname)}");
                }
                else
                {
                    groupNames.Add($"{Client.RemoveDomainPart(group.MailNickname)}");
                }
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
