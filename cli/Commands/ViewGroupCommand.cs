using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Clarius.Edu.CLI
{
    internal class ViewGroupCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresGroup => true;

        public override string Name => "View Group";
        public override string Description => "List basic info about a specified group, including internal profile metadata.";
        internal ViewGroupCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            var grp = Client.GetGroup(Group);

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            Log.Logger.Information("");
            Log.Logger.Information($"MailNickname:          {Group.MailNickname}");
            Log.Logger.Information($"DisplayName:           {Group.DisplayName}");

            if (grp.InternalProfile != null)
            {
                Log.Logger.Information($"Id:                    {grp.Id}");                 
                Log.Logger.Information($"Level:                 {grp.Level}");
                Log.Logger.Information($"Type:                  {grp.Type}");
                Log.Logger.Information($"Grade:                 {grp.Grade}");
                Log.Logger.Information($"Division:              {grp.Division}");
                Log.Logger.Information($"Year:                  {grp.Year}");
            }
            else
            {
                Log.Logger.Warning($"Group doesn't have an internal profile!");
            }
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--viewgroup", "-vg" };
        }

    }
}
