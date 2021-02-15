using System;
using System.Collections.Generic;
using System.Text;

namespace Clarius.Edu.CLI
{
    internal class CommandFactory
    {
        List<ICommand> commands;
        internal CommandFactory(string[] args)
        {
            commands = new List<ICommand>();

            commands.Add(new ListEmailsCommand(args));
            commands.Add(new ListCalendarCommand(args));
            commands.Add(new WhoIsCommand(args));
            commands.Add(new LastLoginsCommand(args));
            commands.Add(new PasswordResetCommand(args));
            commands.Add(new AddToGroupCommand(args));
            commands.Add(new UpdateUserCommand(args));
            commands.Add(new UpdateGroupCommand(args));
            commands.Add(new ListUsersCommand(args));
            commands.Add(new GroupMembersCommand(args));
            commands.Add(new ListGroupsCommand(args));
            commands.Add(new ArchiveTeamCommand(args));
            commands.Add(new SyncGroupsCommand(args));
            commands.Add(new PromoteTeamsCommand(args));
            commands.Add(new SyncUsersCommand(args));
            commands.Add(new CreateUsersCommand(args));
            commands.Add(new ListSectionsCommand(args));
            commands.Add(new ValidateDataCommand(args));
            commands.Add(new ArchiveTeamCommand(args));
        }

        internal List<ICommand> Commands { get { return commands; } }

    }
}
