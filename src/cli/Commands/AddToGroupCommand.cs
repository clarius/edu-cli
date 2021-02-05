using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clarius.Edu.CLI
{
    // edu.exe -atg lista.txt /g:mygroupalias   (you can skip the @domain)
    // edu.exe -atg vga /g:soporteit@ijmecitybell.edu.ar
    
    // you may want to get a list of users beforehand:
    // edu.exe -lu /level:Primaria /type:Docente > lista.txt
    // edu.exe -atg lista.txt /g:nuevoteamparaprimaria
    // (note lista.txt may need clean up before feeding to /atg command)
    internal class AddToGroupCommand : CommandBase
    {
        internal AddToGroupCommand(string[] args) : base(args) { }

        async public override Task RunInternal()
        {
            var groupEmail = base.GetParameterValue("/g:");

            if (groupEmail == null)
            {
                Log.Logger.Information("Missing parameter /g. You must specify the group email address.");
                return;
            }

            var asOwner = GetAsOwner();
            if (asOwner == null)
            {
                return;
            }

            if (!groupEmail.Contains("@"))
            {
                
                groupEmail += $"@{Client.Config.DefaultDomainName}";
            }


            var group = Client.GetGroupFromEmail(groupEmail);
            if (group == null)
            {
                Log.Logger.Error($"Group {groupEmail} not found");
                return;
            }

            var addAsString = asOwner.Value ? "Owner" : "Member";

            if (base.User != null)
            {
                Log.Logger.Information($"Adding User {base.User.DisplayName} to {group.DisplayName} as {addAsString}");
                try
                {
                    await Client.AddUserToGroup(new Guid(group.Id), new Guid(base.User.Id), asOwner.Value);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.Message);
                    Log.Logger.Error($"Failed to add {base.User.DisplayName}");
                }
            }
            else
            {
                foreach (var userLine in base.AliasesList)
                {
                    Log.Logger.Information(userLine);
                    var u = await Client.GetUserFromUserPrincipalName(userLine);
                    Log.Logger.Information($"Adding User {u.DisplayName} to {group.DisplayName} as {addAsString}");
                    try
                    {
                        await Client.AddUserToGroup(new Guid(group.Id), new Guid(u.Id), asOwner.Value);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.Message);
                        Log.Logger.Error($"Failed to add {u.DisplayName}");
                    }
                }
            }
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--AddToGroup", "-atg" };
        }

        bool? GetAsOwner()
        {
            bool asOwner = false;

            foreach (var str in base.Arguments)
            {
                if (str.ToLowerInvariant().StartsWith("/asowner:"))
                {
                    var asOwnerString = str.Substring(9);
                    if (!bool.TryParse(asOwnerString, out asOwner))
                    {
                        Log.Logger.Error($"Invalid value {asOwnerString} for /asowner parameter");
                        return null;
                    }
                }
            }

            return asOwner;
        }

        public override bool UseCache => true;

        public override string Name => "Add To Group";
        public override string Description => "Add a given user or a list of users (text file) to a given group, i.e. edu.exe -atg juan.perez preceptores-inicial or edu.exe -atg usuarios.txt mynewgroup";

    }
}
