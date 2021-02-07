using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Clarius.Edu.CLI
{
    internal class ListEmailsCommand : CommandBase
    {
        public override bool UseCache => false;
        public override bool RequiresUser => true;

        public override bool UseAppPermissions => true;

        internal ListEmailsCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            int count = 0;

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
            .CreateLogger();

            int top = 10;

            if (base.Top != null)
                top = base.Top.Value;


            int c = 0;

            if (!string.IsNullOrEmpty(base.Folder) && base.Folder.Contains("sent", StringComparison.InvariantCultureIgnoreCase))
            {
                var sentFolder = await base.Client.Graph.Users[base.User.Id].MailFolders.SentItems.Messages.Request().Top(top).GetAsync();
                foreach (var email in sentFolder)
                {
                    c++;

                    if (!string.IsNullOrEmpty(base.Filter) && !string.IsNullOrEmpty(email.Subject) && !email.Subject.Contains(base.Filter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(base.From) && email.From != null && !email.From.EmailAddress.Name.Contains(base.From, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (base.Verbose)
                    {
                        Log.Logger.Information($"Folder: Sent\r\nSent: {email.SentDateTime}\r\nFrom: {email.From.EmailAddress.Name}\r\nSubject: {email.Subject}\r\nBody Preview: {email.BodyPreview}");
                        foreach (var to in email.ToRecipients)
                        {
                            Log.Logger.Information($"To: {to.EmailAddress.Name} ({to.EmailAddress.Address})");
                        }
                    }
                    else
                    {
                        Log.Logger.Information($"Sent: {email.SentDateTime}, From: {email.From.EmailAddress.Name}\r\nSubject: {email.Subject}\r\n");
                    }
                }
            }

            if (!string.IsNullOrEmpty(base.Folder) && base.Folder.Contains("inbox", StringComparison.InvariantCultureIgnoreCase))
            {
                var inboxFolder = await base.Client.Graph.Users[base.User.Id].MailFolders.Inbox.Messages.Request().Top(top).GetAsync();
                foreach (var email in inboxFolder)
                {
                    c++;

                    if (!string.IsNullOrEmpty(base.Filter) && !email.Subject.Contains(base.Filter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(base.From) && email.From != null && !email.From.EmailAddress.Name.Contains(base.From, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (base.Verbose)
                    {
                        Log.Logger.Information($"Folder: Inbox\r\nSent: {email.SentDateTime}\r\nFrom: {email.From.EmailAddress.Name}\r\nSubject: {email.Subject}\r\nBody Preview: {email.BodyPreview}");
                        foreach (var to in email.ToRecipients)
                        {
                            Log.Logger.Information($"To: {to.EmailAddress.Name} ({to.EmailAddress.Address})");
                        }
                    }
                    else
                    {
                        Log.Logger.Information($"Sent: {email.SentDateTime}, From: {email.From.EmailAddress.Name}\r\nSubject: {email.Subject}\r\n");
                    }
                }
            }

            if (!string.IsNullOrEmpty(base.Folder) && base.Folder.Contains("deleted", StringComparison.InvariantCultureIgnoreCase))
            {
                var inboxFolder = await base.Client.Graph.Users[base.User.Id].MailFolders.DeletedItems.Messages.Request().Top(top).GetAsync();
                foreach (var email in inboxFolder)
                {
                    c++;

                    if (!string.IsNullOrEmpty(base.Filter) && !email.Subject.Contains(base.Filter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(base.From) && email.From != null && !email.From.EmailAddress.Name.Contains(base.From, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (base.Verbose)
                    {
                        Log.Logger.Information($"Folder: Inbox\r\nSent: {email.SentDateTime}\r\nFrom: {email.From.EmailAddress.Name}\r\nSubject: {email.Subject}\r\nBody Preview: {email.BodyPreview}");
                        foreach (var to in email.ToRecipients)
                        {
                            Log.Logger.Information($"To: {to.EmailAddress.Name} ({to.EmailAddress.Address})");
                        }
                    }
                    else
                    {
                        Log.Logger.Information($"Sent: {email.SentDateTime}, From: {email.From.EmailAddress.Name}\r\nSubject: {email.Subject}\r\n");
                    }
                }
            }

            Log.Logger.Information($"Searched {c} messages");

            return;
        }

        async Task<ICalendarEventsCollectionPage> GetCalendarEventsForUser(string userId, int top = 25)
        {
            return await Client.Graph.Users[userId].Calendar.Events.Request().OrderBy("createdDateTime asc").Top(top).GetAsync();
        }

        private void PrintEvent(Event f)
        {

            if (base.Verbose)
            {
                Log.Logger.Information($"Subject:       {f.Subject}");
                return;
            }
            Log.Logger.Information("------------------------------------------");
#if USE_BETA
            Log.Logger.Information($"UID:           {f.Uid}");
#endif
            Log.Logger.Information($"Type:          {f.Type.Value}");

            Log.Logger.Information($"Subject:       {f.Subject}");
            Log.Logger.Information($"Organizer:     {f.Organizer.EmailAddress.Name} ({f.Organizer.EmailAddress.Address}) ");
            Log.Logger.Information($"LastUpdated:   {f.LastModifiedDateTime}");
            Log.Logger.Information($"Provider:      {f.OnlineMeetingProvider.Value}");

            Log.Logger.Information($"CreatedTime:   {f.CreatedDateTime.Value.LocalDateTime}");
            Log.Logger.Information($"StartTime:     {DateTime.Parse(f.Start.DateTime).ToLocalTime()}");
            Log.Logger.Information($"EndTime:       {DateTime.Parse(f.End.DateTime).ToLocalTime()}");

            Log.Logger.Information($"IsCancelled:   {f.IsCancelled}");
            Log.Logger.Information($"IsOrganizer:   {f.IsOrganizer}");
            Log.Logger.Information($"IsDraft:       {f.IsDraft}");

            foreach (var attendee in f.Attendees)
            {
                Log.Logger.Information($"Attendee:       {attendee.EmailAddress.Address} ({attendee.Type.Value})");
            }

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

        int GetMaxEvents(int top)
        {
            int maxEvents = top;
            if (Top != null)
            {
                maxEvents = Top.Value;
            }

            return maxEvents;
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
            return new List<string>() { "--email", "-e" };
        }

    }
}
