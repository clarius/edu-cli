using Clarius.Edu.Graph;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clarius.Edu.CLI
{
    class Program
    {
        static CommandRouter commandRouter;
        static async Task Main(string[] args)
        {
            var now = DateTime.Now;

            var tempPath = System.IO.Path.GetTempPath();
            var logFilename = $@"{tempPath}\edu-{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}.log";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFilename)
                .CreateLogger();

            commandRouter = new CommandRouter(args);

            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }

            var result = await commandRouter.Execute(args[0]);
        }


        static void PrintHelp()
        {
            Console.WriteLine("Supported commands: edu.exe [command] [username|groupname|/file:] [/v]");
            Console.WriteLine("");

            foreach (var cmd in commandRouter.Commands)
            {
                Console.WriteLine($"Name: {cmd.Name}");
                Console.WriteLine($"Description: {cmd.Description}");
                Console.Write($"Command: ");
                foreach (var c in cmd.GetSupportedCommands())
                {
                    Console.Write($"{c} ");
                }
                Console.WriteLine();
                Console.WriteLine("---");

            }

        }
    }

    internal class CommandRouter
    {
        string[] args;
        internal CommandRouter(string[] args)
        {
            this.args = args;
            Commands = new CommandFactory(args).Commands;
        }

        public List<ICommand> Commands { private set; get; }

        internal async Task<bool> Execute(string commandName)
        {
            foreach (var cmd in Commands)
            {
                if (cmd.CanHandle(commandName))
                {
                    if (!await cmd.Run(this.args, IsVerbose()))
                    {
                        Log.Logger.Error($"Command {commandName} execution failed!");
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }

        bool IsVerbose()
        {
            foreach (var str in args)
            {
                if (str.Equals("/v"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
