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
using Constants = Clarius.Edu.Graph.Constants;

namespace Clarius.Edu.CLI
{
    // TODO: this command is Work in Progress...
    internal class PromoteTeamsCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresGroup => false;
        public override bool RequiresLevel => true;
        public override bool RetrieveEducationUses => true;

        public override string Name => "Promote Teams";
        public override string Description => $"To be run at end of school year, this creates new teams for the upcoming year. Will run on teams of type '{Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS}' only. Required arguments: /level";
        internal PromoteTeamsCommand(string[] args) : base(args)
        {
        }

        async public Task UpdateGroupDisplayName(List<Group> groups, string postfix)
        {
            foreach (var g in groups)
            {
                if (!g.DisplayName.EndsWith(postfix))
                {
                    Log.Logger.Information($"Updating {g.DisplayName}, {g.MailNickname} with {postfix} postfix...");
                    var ng = new Group() { DisplayName = g.DisplayName + postfix };
                    await Client.Graph.Groups[g.Id].Request().UpdateAsync(ng);
                    Log.Logger.Information($"Archiving {g.DisplayName}, {g.MailNickname}...");
                    await Client.Graph.Teams[g.Id].Archive(null).Request().PostAsync();
                }
            }
        }

        async public Task CreateTeams(string level)
        {
            var sections = SchoolManager.GetSections(level);
            int counter = 100;

            foreach (var section in sections)
            {
                foreach (var grade in section.Grades)
                {
                    foreach (var division in section.Divisions)
                    {
                        var students = Client.FilterUsers(Client.GetUsers(), grade, division, null, level, Constants.USER_TYPE_STUDENT);
                        var teamEmail = $"{section.Level.Substring(0, 3)}-{counter}-{grade}{division}";
                        var teamName = string.Format(section.Id, grade, division);

                        if (Client.GetGroupFromDisplayName(teamName) != null)
                        {
                            Log.Logger.Warning($"{teamName} already exists, skipping it");
                            continue;
                        }

                        Log.Logger.Information($"Creating {teamName}, {teamEmail.ToLower()}");
                        var clase = await Client.CreateClass(teamName, teamName, teamEmail.Replace(" ", "").ToLower(), null, null);
                        Thread.Sleep(2000);
                        // add internal profile
                        var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_GROUPGRADE, grade},
                            { Constants.PROFILE_GROUPDIVISION, division},
                            { Constants.PROFILE_GROUPLEVEL, level},
                            { Constants.PROFILE_GROUPTYPE, Constants.GROUP_TYPE_CLASS },
                            { Constants.PROFILE_GROUPYEAR, "2021" },
                            { Constants.PROFILE_GROUPID, teamName },
                        };

                        var grp = await Client.GetGroupFromId(new Guid(clase.Id), false);
                        while (grp == null)
                        {
                            Log.Logger.Warning("Team not ready, retrying...");
                            Thread.Sleep(1500);
                            grp = await Client.GetGroupFromId(new Guid(clase.Id), false);
                        }

                        var vgrp = Client.GetGroup(grp);
                        await vgrp.CreateExtension(Constants.PROFILE_GROUPEXTENSION_ID, customMetadata);
                        Log.Logger.Information("Group extension created!");

                        // add students
                        foreach (var student in students)
                        {
                            var eduStudent = Client.GetEducationUserFromId(new Guid(student.Id));
                            Log.Logger.Information($"adding {eduStudent.UserPrincipalName} to class {clase.DisplayName}");
                            await Client.AddStudent(clase, eduStudent);
                        }

                        // add owners
                        var l = SchoolManager.GetLevel(level);
                        foreach (var owner in l.TeamOwners)
                        {
                            var u = Client.GetEducationUserFromAlias(owner);
                            await Client.AddTeacher(clase, u);
                            Log.Logger.Information($"added teacher {u.DisplayName} to class {clase.DisplayName}");
                        }
                    }
                    counter++;
                }

            }

        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var groups = Client.GetGroups(Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS, base.Level);

            // Update DisplayName to include past year postfix (e.g. " (2020)") and archive teams
          //  await UpdateGroupDisplayName(groups, " (2020)");

            // Create new temas based on sections (NOTE: this MUST run AFTER students profile has been updated for next year)
            await CreateTeams(base.Level);
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--promoteteams", "-pt" };
        }
    }
}
