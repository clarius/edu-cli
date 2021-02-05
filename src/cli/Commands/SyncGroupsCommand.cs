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
    // TODO: this command is Work in Progress...
    internal class SyncGroupsCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresGroup => false;

        public override string Name => "Sync Groups";
        public override string Description => $"Checks and updates internal metadata for groups. Will run on groups of type '{Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS}' only";
        internal SyncGroupsCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            await Update_Groups_Secundaria();
            //await Update_Groups_Inicial_Level();
        }

        async public Task RunInternal2()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var groups = Client.GetGroups(Constants.GROUP_TYPE_CLASS, Constants.LEVEL_INICIAL);

            int missingstuff = 0;

            foreach (var g in groups)
            {
                var vg = Client.GetGroup(g);
                Log.Logger.Information(g.DisplayName);
                if (vg.Type == null)
                {
                    Log.Logger.Warning("Missing 'Type' attribute");
                    missingstuff++;
                }
                else
                    Log.Logger.Information(vg.Type);
                if (vg.Level == null)
                {
                    Log.Logger.Warning("Missing 'Level' attribute");
                    missingstuff++;
                }
                else
                    Log.Logger.Information(vg.Level);
                if (vg.Grade == null)
                {
                    Log.Logger.Warning("Missing 'Grade' attribute");
                    missingstuff++;
                }
                else
                    Log.Logger.Information(vg.Grade);
                if (vg.Division == null)
                {
                    Log.Logger.Warning("Missing 'Division' attribute");
                    missingstuff++;
                }
                else
                    Log.Logger.Information(vg.Division);

                Log.Logger.Information("---");
            }

            Log.Logger.Information("");
            Log.Logger.Information("---");
            Log.Logger.Warning($"Missing {missingstuff} total instances");

            return;

            //Log.Logger.Information("");
            //Log.Logger.Information($"MailNickname:          {Group.MailNickname}");
            //Log.Logger.Information($"DisplayName:           {Group.DisplayName}");

            //if (grp.InternalProfile != null)
            //{
            //    Log.Logger.Information($"Level:                 {grp.Level}");
            //    Log.Logger.Information($"Type:                  {grp.Type}");
            //    Log.Logger.Information($"Grade:                 {grp.Grade}");
            //    Log.Logger.Information($"Division:              {grp.Division}");
            //}
            //else
            //{
            //    Log.Logger.Warning($"Group doesn't have an internal profile!");
            //}
        }

        async public Task Update_Groups_Secundaria()
        {

            Log.Logger.Information($"Starting SyncGroupsCommand");
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var groups = Client.GetGroups(Constants.GROUP_TYPE_CLASS, Constants.LEVEL_SECUNDARIA);

            int c = 0;

            foreach (var g in groups)
            {
                c++;
                    //Log.Logger.Information($"{g.DisplayName}");
                var grade = GetGradeFromDisplayName(g.DisplayName);
                var division = GetDivisionFromDisplayName(g.DisplayName);
                if (grade == null)
                {
                    Log.Logger.Error($"{g.DisplayName}, {g.MailNickname} does not contain a grade information");
                    continue;
                }

                var vg = Client.GetGroup(g);

                if (vg == null)
                    continue;

                if (vg.Grade != grade || vg.Division != division)
                {
                    Log.Logger.Warning($"{g.DisplayName} mismatch: {vg.Grade} vs. {grade}, {vg.Division} vs {division}");
                }
            }

            Log.Logger.Information($"processed {c} groups...");
        }

        string  GetDivisionFromDisplayName(string displayName)
        {
            if (displayName.Contains(" A ") || displayName.EndsWith(" A"))
            {
                return "A";
            }
            if (displayName.Contains(" B ") || displayName.EndsWith(" B"))
            {
                return "B";
            }
            if (displayName.Contains(" C ") || displayName.EndsWith(" C"))
            {
                return "C";
            }
            if (displayName.Contains("Nivel 1"))
            {
                return "A";
            }
            if (displayName.Contains("Nivel 2"))
            {
                return "B";
            }
            if (displayName.Contains("C"))
            {
                return "Nivel 3";
            }

            return null;
        }

        string GetGradeFromDisplayName(string displayName)
        {
            foreach (var g in SchoolManager.Grades)
            {
                if (displayName.Contains(g, StringComparison.InvariantCultureIgnoreCase))
                {
                    return g;
                }
            }

            return null;
        }

        async public Task Update_Groups_Inicial_Level()
        {

            Log.Logger.Information($"Starting SyncGroupsCommand");
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var groups = Client.GetGroups(Constants.GROUP_TYPE_CLASS, Constants.LEVEL_INICIAL);

            string grade = null;
            string level = null;

            int c = 0;

            foreach (var g in groups)
            {
                var vg = Client.GetGroup(g);
             //   Log.Logger.Information($"{g.DisplayName}, {g.MailNickname}");

                // sample address: inicial345-3eroC
                var parts = g.MailNickname.Split("-");

                if (parts.Length != 2)
                {
                    Log.Logger.Error($"{g.DisplayName} - Found {parts.Length} parts in {g.MailNickname}, must be only 2");
                    return;
                }

                string division;
                if (parts[1].EndsWith("A", StringComparison.InvariantCultureIgnoreCase))
                {
                    division = "A";
                }
                else if (parts[1].EndsWith("B", StringComparison.InvariantCultureIgnoreCase))
                {
                    division = "B";
                }
                else if (parts[1].EndsWith("C", StringComparison.InvariantCultureIgnoreCase))
                {
                    division = "C";
                }
                else
                {
                    Log.Logger.Error($"{g.DisplayName} - {parts[1]} does not end with a Division char");
                    return;
                }

                foreach (var str in SchoolManager.Grades)
                {
                    if (parts[1].Contains(str, StringComparison.InvariantCultureIgnoreCase))
                        grade = str;
                }

                if (grade == null)
                {
                    Log.Logger.Error($"{g.DisplayName} - {parts[1]} does not start with a Grade");
                    return;
                }

                string type = null;

                var groupId = RemovePersonalNames(g.DisplayName);
                groupId = $"{groupId} {vg.Grade} {vg.Division}";

                Log.Logger.Information($"groupId = {groupId}");

                var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_GROUPGRADE, grade ??= vg.Grade },
                            { Constants.PROFILE_GROUPDIVISION, division ??= vg.Division },
                            { Constants.PROFILE_GROUPLEVEL, level ??= vg.Level },
                            { Constants.PROFILE_GROUPTYPE, type ??= Constants.GROUP_TYPE_CLASS },
                            { Constants.PROFILE_GROUPYEAR, "2020" },
                            { Constants.PROFILE_GROUPID, groupId },
                        };
                try
                {
                    Log.Logger.Information($"Grade:     {grade}");
                    Log.Logger.Information($"Division:  {division}");
                    Log.Logger.Information($"Level:     {level}");
                    Log.Logger.Information($"Type:      {type}");
                    Log.Logger.Information($"Id:        {groupId}");

                    await vg.UpdateExtension(Constants.PROFILE_GROUPEXTENSION_ID, customMetadata);
                    Log.Logger.Information("Group extension updated!");
                }
                catch (Exception ex)
                {
                    Log.Logger.Warning($"{ex.Message}");
                }

                c++;
                //if (c > 10)
                //    break;
            }

            Log.Logger.Information($"Updated {c} groups...");
        }

        private string RemovePersonalNames(string displayName)
        {
            List<string> names = new List<string> { " Eugenia", " Florencia", " Jimena", " Euge", " Agustina", " Leila", " Flor", " Mariu", " Soledad", " Sabrina" };
            string name = displayName;

            foreach (var n in names)
            {
                name = name.Replace(n, "");
            }

            return name;
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--syncgroups", "-sg" };
        }

    }
}
