using System;
using System.Globalization;

namespace dailies.Client.Models
{
    public class DateUtilities
    {
        public string GetRelativeDateFromToday(DateTime date)
        {
            var span = date.Date - DateTime.Today;
            var inDays = (int)span.TotalDays;

            switch (inDays)
            {
                case int n when (n == -1):
                    return "Yesterday";

                case int n when (n < 0):
                    return $"{inDays * -1} days ago";

                case int n when (n == 0):
                    return "Today";

                case int n when (n == 1):
                    return "Tomorrow";

                case int n when (n > 1):
                    return $"in {inDays} days";

                default:
                    return null;
            }
        }

        public string GetStandardShortDate(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public DateTime? ParseStandardShortDate(string dateString)
        {
            if (dateString == null) return null;

            var parseResult = DateTime.TryParseExact(
                dateString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var parsed
            );

            if (parseResult) return parsed;
            return null;
        }
    }
}