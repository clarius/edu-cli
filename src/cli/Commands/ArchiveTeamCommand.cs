using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Clarius.Edu.CLI
{
    internal class ArchiveTeamCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresGroup => true;
        public override bool RequiresValue => true;

        public override string Name => "Archive Team";
        public override string Description => "Archive/unarchive a Team. Usage: edu.exe groupemail /value:[true|false]";
        internal ArchiveTeamCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            var grp = Client.GetGroup(Group);

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var action = base.Value ? "Archiving" : "Unarchiving";
            Log.Logger.Information($"{action} team {Group.DisplayName}...");

            await grp.Archive(base.Value);
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--archiveteam", "-at" };
        }

    }
}
