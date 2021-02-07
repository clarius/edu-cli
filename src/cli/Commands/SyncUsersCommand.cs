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
    internal class SyncUsersCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresGroup => false;

        public override string Name => "Sync Users";
        public override string Description => "Checks and updates internal metadata for users.";
        internal SyncUsersCommand(string[] args) : base(args)
        {
        }

        async public Task RunInternal2()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var groups = Client.GetGroups(Clarius.Edu.Graph.Constants.GROUP_TYPE_CLASS, Clarius.Edu.Graph.Constants.LEVEL_SECUNDARIA);

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

        async public override Task RunInternal()
        {
            await Run_TestPromotion();

            return;

            string newGrade, newLevel;
            SchoolManager.GetPromotedGradeAndLevel("6to", Clarius.Edu.Graph.Constants.LEVEL_PRIMARIA, out newGrade, out newLevel);

            Log.Logger.Information($"Bumping from 6to Primaria to {newGrade} {newLevel}");
        }

        async public Task Run_ResetUsersGradeAndDivision()
        {
            // this will take the members of several groups of level Primaria and reset their internal metadata, specifically, grade and division, in case it ever gets overwritten by mistake
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            // note: the below group names may change when they get archived (ie. to contain a (2020) postfix) -- the group metatada will always contain a "Year" field with the right year specified in there
            var groupsPrimaria = new List<string> { "Computación 1ero A","Computación 1ero B","Computación 1ero C", "Computación 2do A","Computación 2do B","Computación 2do C", "Computación 3ero A","Computación 3ero B","Computación 3ero C","Computación 4to A","Computación 4to B","Computación 4to C",
            "Computación 5to A","Computación 5to B","Computación 5to C","Computación 6to A","Computación 6to B","Computación 6to C"};


            foreach (var groupName in groupsPrimaria)
            {
                var group = Client.GetGroupByIdAndLevel(groupName, Clarius.Edu.Graph.Constants.LEVEL_PRIMARIA);

                //  Log.Logger.Information($"{group.DisplayName}");

                var members = await Client.Graph.Groups[group.Id].Members.Request().GetAsync();

                int c = 0;

                foreach (var m in members)
                {
                    var vg = Client.GetGroup(group);
                    var u = Client.GetUserFromId(new Guid(m.Id));
                    var vu = Client.GetUser(u);

                    // Log.Logger.Information($"{u.DisplayName} {vu.Grade} {vu.Division}, {vu.Year} change to: { vg.Grade} { vg.Division}");

                    var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_USERGRADE, vg.Grade },
                            { Constants.PROFILE_USERDIVISION, vg.Division },
                            { Constants.PROFILE_USERLEVEL, vu.Level },
                            { Constants.PROFILE_USERTYPE, vu.Type },
                            { Constants.PROFILE_USERENGLISHLEVEL, vu.EnglishLevel },
                            { Constants.PROFILE_USERYEAR, vu.Year },
                        };
                    try
                    {
                        // uncomment below to actually update the users metadata
                        // await vu.UpdateExtension(Constants.EXTENSION_ID, customMetadata);

                        Log.Logger.Information($"{Client.RemoveDomainPart(u.UserPrincipalName)},{vg.Grade},{vg.Division},{vg.Level},{vu.Type},{vg.Year}");
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Warning($"{ex.Message}");
                    }

                    c++;
                    //if (c > 0)
                    //    break;
                }
            }

            /*
            var users = Client.GetUsers(Clarius.Edu.Graph.Constants.USER_TYPE_STUDENT, Clarius.Edu.Graph.Constants.GROUP_LEVEL_PRIMARIA);

            string grade = null;
            string division = null;
            string level = null;

            int c = 0;

            foreach (var u in users)
            {
                var vu = Client.GetUser(u);
                Log.Logger.Information($"{u.DisplayName}, {u.MailNickname}");

                string type = null;

                var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.USERGRADE, grade ??= vu.Grade },
                            { Constants.USERDIVISION, division ??= vu.Division },
                            { Constants.USERLEVEL, level ??= vu.Level },
                            { Constants.USERTYPE, type ??= vu.Type },
                            { Constants.USERYEAR, "2020" },
                            { Constants.USERENGLISHLEVEL, vu.EnglishLevel },
                        };
                try
                {
                    await vu.UpdateExtension(Constants.EXTENSION_ID, customMetadata);
                    Log.Logger.Information("User extension updated!");
                }
                catch (Exception ex)
                {
                    Log.Logger.Warning($"{ex.Message}");
                }

                c++;
                //if (c > 0)
                //    break;
            }

            Log.Logger.Information($"Updated {c} users..."); */
        }

        async public Task Run_TestPromotion()
        {
            // this will take the members of several groups of level Primaria and reset their internal metadata, specifically, grade and division, in case it ever gets overwritten by mistake

            Log.Logger.Information($"Starting SyncUsersCommand");
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            var users = Client.GetUsers(Constants.USER_TYPE_STUDENT, Constants.LEVEL_PRIMARIA);

            var newYear = "2021";
            int c = 0;

            foreach (var m in users)
            {
                var vu = Client.GetUser(m);

                string newGrade; string newLevel;
                if (!SchoolManager.GetPromotedGradeAndLevel(vu.Grade, vu.Level, out newGrade, out newLevel))
                {
                    Log.Logger.Warning($"Failed to promote {m.MailNickname} from {vu.Grade} {vu.Level}");
                }


                // Log.Logger.Information($"{u.DisplayName} {vu.Grade} {vu.Division}, {vu.Year} change to: { vg.Grade} { vg.Division}");

                var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_USERGRADE, newGrade },
                            { Constants.PROFILE_USERDIVISION, vu.Division },
                            { Constants.PROFILE_USERLEVEL, newLevel },
                            { Constants.PROFILE_USERTYPE, vu.Type },
                            { Constants.PROFILE_USERENGLISHLEVEL, vu.EnglishLevel },
                            { Constants.PROFILE_USERYEAR, newYear },
                        };
                try
                {
                    // uncomment below to actually update the users metadata
                    await vu.UpdateExtension(Constants.PROFILE_USEREXTENSION_ID, customMetadata);

                    Log.Logger.Information($"{Client.RemoveDomainPart(m.UserPrincipalName)},{newGrade},{vu.Division},{newLevel},{vu.Type},{newYear}");
                }
                catch (Exception ex)
                {
                    Log.Logger.Warning($"{ex.Message}");
                }

                c++;
                if (c > 0)
                    break;
            }
        }

        /*
        var users = Client.GetUsers(Clarius.Edu.Graph.Constants.USER_TYPE_STUDENT, Clarius.Edu.Graph.Constants.GROUP_LEVEL_PRIMARIA);

        string grade = null;
        string division = null;
        string level = null;

        int c = 0;

        foreach (var u in users)
        {
            var vu = Client.GetUser(u);
            Log.Logger.Information($"{u.DisplayName}, {u.MailNickname}");

            string type = null;

            var customMetadata = new Dictionary<string, object>
                    {
                        { Constants.USERGRADE, grade ??= vu.Grade },
                        { Constants.USERDIVISION, division ??= vu.Division },
                        { Constants.USERLEVEL, level ??= vu.Level },
                        { Constants.USERTYPE, type ??= vu.Type },
                        { Constants.USERYEAR, "2020" },
                        { Constants.USERENGLISHLEVEL, vu.EnglishLevel },
                    };
            try
            {
                await vu.UpdateExtension(Constants.EXTENSION_ID, customMetadata);
                Log.Logger.Information("User extension updated!");
            }
            catch (Exception ex)
            {
                Log.Logger.Warning($"{ex.Message}");
            }

            c++;
            //if (c > 0)
            //    break;
        }

        Log.Logger.Information($"Updated {c} users..."); */

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--syncusers", "-su" };
        }
    }

}
