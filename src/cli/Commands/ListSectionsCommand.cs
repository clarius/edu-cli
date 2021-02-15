using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.Design;
using Clarius.Edu.Graph;

namespace Clarius.Edu.CLI
{
    internal class ListSectionsCommand : CommandBase
    {
        public override bool UseCache => false;
        public override bool RequiresLevel => true;

        public override string Name => "List Sections";
        public override string Description => "List all sections for a given level. Required arguments: /level";
        internal ListSectionsCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            int c = 0;

            foreach (var s in Graph.SchoolManager.GetSections(base.Level))
            {
                foreach (var name in s.GetNames())
                {
                    Console.WriteLine($"{name}");
                    c++;
                }
            }

            Log.Logger.Information($"Total count: {c}");
            return;
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--listsections", "-ls" };
        }

    }
}
