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
    internal class UpdateGroupCommand : CommandBase
    {
        public override bool UseCache => true;

        internal UpdateGroupCommand(string[] args) : base (args)
        {
        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var groupsToProcess = new List<Microsoft.Graph.Group>();
            if (AliasesList != null)
            {
                foreach (var groupLine in AliasesList)
                {
                    var g = await Client.GetGroupFromAlias(groupLine);
                    if (g != null)
                    {
                        groupsToProcess.Add(g);
                    }
                    else
                    {
                        Log.Logger.Warning($"Cannot find group {groupLine}");
                    }
                }
            }
            else
            {
                if (Group != null)
                    groupsToProcess.Add(Group);
            }

            if (groupsToProcess.Count > 0)
            {
                await ProcessGroups(groupsToProcess);
                return;
            }

            Log.Logger.Error("You must specify either a group alias or group alias list (text file, using /file:) for this command to work");
        }

        async Task ProcessGroups(List<Microsoft.Graph.Group> groupsToProcess)
        {
            var newLevel = base.GetParameterValue("/level:");
            var newType = base.GetParameterValue("/type:");
            var newDivision = base.GetParameterValue("/division:");
            var newGrade = base.GetParameterValue("/grade:");
            var newYear = base.GetParameterValue("/year:");
            var newId = base.GetParameterValue("/id:");

            Log.Logger.Information($"Going to process {groupsToProcess.Count} groups...");

            foreach (var group in groupsToProcess)
            {
                Log.Logger.Information("");
                Log.Logger.Information($"MailNickname:          {group.MailNickname}");
                Log.Logger.Information($"DisplayName:           {group.DisplayName}");

                var grp = Client.GetGroup(group);

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
                    Log.Logger.Error($"Group doesn't have an internal profile!");
                }

                if (grp.InternalProfile == null)
                {
                    var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_GROUPGRADE, newGrade ?? ""},
                            { Constants.PROFILE_GROUPDIVISION, newDivision ?? ""},
                            { Constants.PROFILE_GROUPLEVEL, newLevel ?? "" },
                            { Constants.PROFILE_GROUPTYPE, newType ?? "" },
                            { Constants.PROFILE_GROUPYEAR, newYear ?? "" },
                            { Constants.PROFILE_GROUPID, newId ?? "" },
                        };
                    await grp.CreateExtension(Constants.PROFILE_GROUPEXTENSION_ID, customMetadata);
                    Log.Logger.Information("Group extension created!");
                }
                else
                {
                    Log.Logger.Information($"{group.MailNickname} found");
                    var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_GROUPGRADE, newGrade ?? grp.Grade },
                            { Constants.PROFILE_GROUPDIVISION, newDivision ?? grp.Division },
                            { Constants.PROFILE_GROUPLEVEL, newLevel ?? grp.Level },
                            { Constants.PROFILE_GROUPTYPE, newType ?? grp.Type },
                            { Constants.PROFILE_GROUPYEAR, newYear ?? grp.Year },
                            { Constants.PROFILE_GROUPID, newId ?? grp.Id },
                        };
                    try
                    {
                        await grp.UpdateExtension(Constants.PROFILE_GROUPEXTENSION_ID, customMetadata);
                        Log.Logger.Information("User extension updated!");
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Warning($"{ex.Message}");
                    }
                }
            }
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--updategroup", "-ug" };
        }

        public override string Name => "Update Group";
        public override string Description => "Updates the metadata of a given group";
    }
}
