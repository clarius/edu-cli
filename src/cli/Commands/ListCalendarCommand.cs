using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Clarius.Edu.Graph;

namespace Clarius.Edu.CLI
{
    internal class ListCalendarCommand : CommandBase
    {
        public override bool UseCache => false;
        public override bool RequiresUser => true;

        internal ListCalendarCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            int count = 0;

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            if (base.Organizer != null)
            {
                // if /organizer was specified then we assume we need to look into one or more users calendars for meeting with the given organizer
                List<string> usersToSearch = new List<string>();

                if (base.User != null)
                    usersToSearch.Add(Client.RemoveDomainPart(base.User.UserPrincipalName));
                else
                {
                    usersToSearch.AddRange(base.AliasesList);
                }

                int hits = 0;
                int misses = 0;
                int userCount = 0;
                foreach (var userLine in usersToSearch)
                {
                    userCount++;
                    var u = await Client.GetUserFromUserPrincipalName(userLine);
                    Log.Logger.Information($"Looking into {u.DisplayName}'s calendar...");
                    try
                    {
                        bool meetingFound = false;

                        int maxEvents = GetMaxEvents(100);
                        var events = await GetCalendarEventsForUser(u.Id, maxEvents);
                        foreach (var @event in events)
                        {
                            if (@event.Organizer.EmailAddress.Address.Contains(base.Organizer, StringComparison.InvariantCultureIgnoreCase))
                            {
                                Log.Logger.Information($"Subject: {@event.Subject} / Organizer: {@event.Organizer.EmailAddress.Name}");
                                meetingFound = true;
                            }
                        }
                        if (!meetingFound)
                        {
                            Log.Logger.Warning($"User {u.DisplayName} does not have this meeting");
                            misses++;
                        }
                        else
                        {
                            hits++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.Message);
                    }
                }
                Log.Logger.Information($"{hits} users have this meeting");
                Log.Logger.Information($"{misses} users have NOT this meeting");
                Log.Logger.Information($"{userCount} total users searched");
            }
            else
            {
                var usr = Client.GetUser(base.User);

                Log.Logger.Information($"Level:     {usr.Level}");
                Log.Logger.Information($"Type:      {usr.Type}");

                int maxEvents = GetMaxEvents(25);

                var events = await GetCalendarEventsForUser(User.Id, maxEvents);
                foreach (var f in events)
                {
                    if (base.Filter != null)
                    {
                        if (f.Subject != null && !f.Subject.Contains(base.Filter, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                    }
                    if (base.Id != null)
                    {
                        if (f.Id != null && !f.Id.Equals(base.Id, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                    }
                    PrintEvent(f);
                    count++;
                }
                Log.Logger.Information("");
                Log.Logger.Information($"Total meetings listed: {count}");
            }

        }

        async Task<ICalendarEventsCollectionPage> GetCalendarEventsForUser(string userId, int top = 25)
        {
            return await Client.Graph.Users[userId].Calendar.Events.Request().OrderBy("createdDateTime asc").Top(top).GetAsync();
        }

        private void PrintEvent(Event f)
        {
            
            Log.Logger.Information("------------------------------------------");

            if (base.Verbose)
            {
#if USE_BETA
                Log.Logger.Information($"UID:           {f.Uid}");
#endif
                Log.Logger.Information($"Type:          {f.Type.Value}");
            }

#if USE_BETA
            Log.Logger.Information($"Subject:       {f.Subject} ({f.Uid})");
#endif
            
            if (base.Verbose)
            {
                Log.Logger.Information($"Organizer:     {f.Organizer.EmailAddress.Name} ({f.Organizer.EmailAddress.Address}) ");
                Log.Logger.Information($"LastUpdated:   {f.LastModifiedDateTime}");
                Log.Logger.Information($"Provider:      {f.OnlineMeetingProvider.Value}");

                Log.Logger.Information($"CreatedTime:   {f.CreatedDateTime.Value.LocalDateTime}");
                Log.Logger.Information($"StartTime:     {DateTime.Parse(f.Start.DateTime).ToLocalTime()}");
                Log.Logger.Information($"EndTime:       {DateTime.Parse(f.End.DateTime).ToLocalTime()}");

                Log.Logger.Information($"IsCancelled:   {f.IsCancelled}");
                Log.Logger.Information($"IsOrganizer:   {f.IsOrganizer}");
                Log.Logger.Information($"IsDraft:       {f.IsDraft}");
            }

            if (base.Verbose)
            {
                foreach (var attendee in f.Attendees)
                {
                    Log.Logger.Information($"Attendee:       {attendee.EmailAddress.Address} ({attendee.Type.Value})");
                }
            }

            if (base.Verbose)
            {
                Log.Logger.Information($"BodyPreview:   {f.BodyPreview}");

                if (f.Recurrence != null)
                {
                    Log.Logger.Information($"Type:          {f.Recurrence.Pattern.Type}");
                    Log.Logger.Information($"Interval:      {f.Recurrence.Pattern.Interval}");
                    Log.Logger.Information($"Index:         {f.Recurrence.Pattern.Index}");
                    if (f.Recurrence.Pattern.DaysOfWeek != null)
                    {
                        var daysOfWeek = GetDaysOfWeek(f.Recurrence.Pattern.DaysOfWeek);
                        Log.Logger.Information($"DaysOfWeek:    {daysOfWeek}");
                    }
                    Log.Logger.Information($"Month:         {f.Recurrence.Pattern.Month}");
                    Log.Logger.Information($"1stDayOfWeek:  {f.Recurrence.Pattern.FirstDayOfWeek}");
                    Log.Logger.Information($"Range:         {f.Recurrence.Range.StartDate} to {f.Recurrence.Range.EndDate} ");
                }
            }

        }

        int GetMaxEvents(int top)
        {
            int maxEvents = top;
            if (Top != null)
            {
                maxEvents = Top.Value;
            }

            return maxEvents;
        }

        public async Task FindMeetings()
        {
            Log.Logger.Information($"Starting PeekUserCommand");

            var client = new Client("ijme.config", false);

            await client.Connect();

            var teams = await client.GetTeams(999);

            var groupId = "0167147d-f8d1-468b-8096-309259d11d49";

            var g1 = await client.Graph.Groups[groupId].Request().GetAsync();
            var o1 = await client.Graph.Groups[groupId].Owners.Request().GetAsync();

            var users = client.GetTeachers("Inicial");
            int hitCount = 0;
            int userCount = 0;

            foreach (var user in users)
            {

                Log.Logger.Information($"Looking at user: {userCount++}: {user.DisplayName}...");
                var foo = await client.Graph.Users[user.Id].Calendar.Events.Request().GetAsync();
                bool found = false;
                foreach (var f in foo)
                {
                    Log.Logger.Information($"Subject: {f.Subject}");
                    if (f.Subject == null)
                    {
                        Log.Logger.Error($"subject is null at {user.DisplayName}");
                    }
                    else if (f.Subject.ToLower().Contains("horas de consulta"))
                    {
                        Log.Logger.Information("Meeting found!");
                        found = true;
                        hitCount++;
                        break;
                    }
                }
                if (!found)
                {
                    Log.Logger.Warning($"User {user.DisplayName} missing this meeting");
                }

            }

            Log.Logger.Information("");
            Log.Logger.Information($"{hitCount} out of {userCount} users have this meeting");

            return;
        }

        private string GetDaysOfWeek(IEnumerable<Microsoft.Graph.DayOfWeek> daysOfWeek)
        {
            var sb = new StringBuilder();

            foreach (var d in daysOfWeek)
            {
                sb.Append(d + ", ");
            }

            return sb.ToString().Substring(0, sb.Length - 2);
        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--calendar", "-c" };
        }

        public override string Name => "List Calendar";
        public override string Description => "List calendar entries for a given user, i.e. edu.exe -c juan.perez";
    }
}
