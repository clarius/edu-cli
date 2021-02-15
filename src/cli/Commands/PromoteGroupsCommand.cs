using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Clarius.Edu.Graph;
using System.Threading;
using System.Reflection.Metadata;
using System.Linq.Expressions;

namespace Clarius.Edu.CLI
{
    // TODO: this command is Work in Progress...
    internal class PromoteGroupCommand : CommandBase
    {
        public override bool RetrieveEducationUses => true;
        public override bool UseCache => true;
        public override bool RequiresGroup => false;
        public override bool RequiresLevel => true;

        public override string Name => "Promote Groups";
        public override string Description => $"To be run at end of school year, this creates new groups for the upcoming year. Will run on groups of type '{Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS}' only. Required arguments: /level";
        internal PromoteGroupCommand(string[] args) : base(args)
        {
        }

        async public Task UpdateGroupDisplayName(string level, string postfix)
        {
            var groups = Client.GetGroups(Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS, level);

            int c = 0;

            foreach (var g in groups)
            {
                if (!g.DisplayName.EndsWith(" (2020)"))
                {
                    Log.Logger.Information($"Updating {g.DisplayName}, {g.MailNickname} with 2020 postfix...");
                    var ng = new Group() { DisplayName = g.DisplayName + " (2020)" };
                    await Client.Graph.Groups[g.Id].Request().UpdateAsync(ng);
                }
            }
        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            // Update DisplayName to include past year postfix (e.g. " (2020)")
            await UpdateGroupDisplayName(Graph.Constants.LEVEL_INICIAL, " (2020)");
            await UpdateGroupDisplayName(Graph.Constants.LEVEL_PRIMARIA, " (2020)");
            await UpdateGroupDisplayName(Graph.Constants.LEVEL_SECUNDARIA, " (2020)");

            // int c = 0;

            // Create new sections for upcoming school year
            //int counter = 100;

            //var sections = SchoolManager.GetSections(Graph.Constants.LEVEL_PRIMARIA);

            //foreach (var section in sections)
            //{
            //    // we don't want to process the last grade (usually '6to') as it may imply a bump in level too (i.e. from Primaria to Secundaria)
            //    for (int c=0; c < section.Grades.Count - 1; c++)
            //    {
            //        foreach (var division in section.Divisions)
            //        {
            //            Log.Logger.Information($"looking for {section.Grades[c]} {division} in {section.Id}");

            //            var s = Client.GetGroupByIdAndLevel(string.Format(section.Id, section.Grades[c], division), section.Level);
            //            var vGroup = Client.GetGroup(s);
            //            Log.Logger.Information($"{s.DisplayName} is going to be promoted!");

            //            var email = $"{Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS}{counter++}-{section.Level}";

            //            if (c==0)
            //            {
            //                continue; // we don't want to do much processing for the first year section as students need to come from a different level (e.g. 1er Grado -> from -> salita jardin)
            //            }

            //            var clase = await Client.CreateClass(vGroup.Id, s.Description, email, null, null);
            //            Thread.Sleep(1000);



            //        }
            //    }
        }

        /*
            //if (section.PromoteToId == null)
            //{
            //    Log.Logger.Information($"{section.Id} -> no promotion Id specified");
            //    continue;
            //}

          //  Log.Logger.Information($"{section.Id} -> {section.PromoteToId}");

            var s = Client.GetGroupByIdAndLevel(section.Id, section.Level);
            var vGroup = Client.GetGroup(s);

            Log.Logger.Information($"{s.DisplayName} is going to be promoted!");

            var clase = await Client.CreateClass(vGroup.Id, s.Description, "", section.Grade, null);

            Thread.Sleep(1000);




            var members = await Client.Graph.Groups[s.Id].Members.Request().GetAsync();

            foreach (var member in members)
            {
                var usr = Client.GetUserFromId(new Guid(member.Id));
                Log.Logger.Information($"{usr.DisplayName}, {usr.Id}");
            }

            var owners = await Client.Graph.Groups[s.Id].Owners.Request().GetAsync();

            //foreach (var owner in owners)
            //{
            //    Log.Logger.Information($"{owner.Id}");
            //    await Client.AddTeacher()
            //    //var usr = Client.GetUserFromId(new Guid(owner.Id));
            //    //Log.Logger.Information($"{usr.DisplayName}, {usr.Id}");
            //}



            // get a handle of our nice group wrapper for high level operations like ChangeDisplayName, Archive, etc.
            var oldGroup = Client.GetGroup(s);
            // the group's display name will be postfixed with "(2020)" where 2020 is actually taken from the groups metadata 'Year' which is the school year the group was used
            await oldGroup.ChangeDisplayName($"{s.DisplayName} ({oldGroup.Year})");
            // now archive the group so it becomes read-only
            await oldGroup.Archive(true);

            //Client.CreateGroup(oldGroup.Id, "", "", )
         //   Client.CreateClass(s.DisplayName, "", "", )

        }



        return;

        //foreach (var g in groups)
        //{
        //    var vg = Client.GetGroup(g);
        //    Log.Logger.Information($"{g.DisplayName}, {g.MailNickname}");

        //    var parts = g.DisplayName.Split(" ");

        //    foreach (var f in parts)
        //    {
        //        if (Client.ValidateGrade(f))
        //        {
        //            grade = f;
        //            Log.Logger.Information($"Grade: {grade}");
        //        }
        //        else if (Client.ValidateDivision(f))
        //        {
        //            division = f;
        //            Log.Logger.Information($"Division: {division}");
        //        }
        //        else if (Client.ValidateLevel(f))
        //        {
        //            level = f;
        //            Log.Logger.Information($"Level: {level}");
        //        }
        //        //Log.Logger.Information(f);
        //    }

        //    string type = null;

        //    var customMetadata = new Dictionary<string, object>
        //            {
        //                { Constants.GROUPGRADE, grade ??= vg.Grade },
        //                { Constants.GROUPDIVISION, division ??= vg.Division },
        //                { Constants.GROUPLEVEL, level ??= vg.Level },
        //                { Constants.GROUPTYPE, type ??= "Clase" },
        //                { Constants.GROUPYEAR, "2020" },
        //                { Constants.GROUPID, g.DisplayName },
        //            };
        //    try
        //    {
        //        Log.Logger.Information($"Grade:     {grade}");
        //        Log.Logger.Information($"Division:  {division}");
        //        Log.Logger.Information($"Level:     {level}");
        //        Log.Logger.Information($"Type:      {type}");
        //        await vg.UpdateExtension(Constants.EXTENSION_ID, customMetadata);
        //        Log.Logger.Information("Group extension updated!");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Warning($"{ex.Message}");
        //    }

        //    c++;
        //    //if (c > 0)
        //    //    break;
        //}


        //  Log.Logger.Information($"Updated {c} groups...");
        */

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--promotegroups", "-pg" };
        }
    }
}
