using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using dailies.Shared;

namespace dailies.Client.Models
{
    public class EntriesManager
    {
        public List<(int year, int month)> FetchedMonths { get; private set; } = new List<(int year, int month)>();
        public Dictionary<DateTime, Entry> Entries { get; private set; } = new Dictionary<DateTime, Entry>();
        public HttpClient Http { get; }

        public EntriesManager(HttpClient Http)
        {
            this.Http = Http;
        }

        // Get an entry
        // Automatically fetch required data if needed
        public async Task<Entry> GetEntryAsync(DateTime date)
        {
            // Already fetched - Shortcut and return fetched data
            if (FetchedMonths.Any(x => x.month == date.Month && x.year == date.Year))
            {
                return GetEntryOrNull(date);
            }
            else
            {
                // Data not fetched yet.
                // Fetch and return
                await FetchEntriesForMonthAsync(date.Year, date.Month);
                return GetEntryOrNull(date);
            }
        }

        private Entry GetEntryOrNull(DateTime date)
        {
            if (Entries.ContainsKey(date.Date))
            {
                // Entry found
                return Entries[date.Date];
            }
            else
            {
                // No entry for this date
                return null;
            }
        }

        /// Fetch data for given month and adjacent ones
        public async Task FetchEntriesToDisplayForMonthAsync(int year, int month)
        {
            await FetchEntriesForMonthAsync(year, month);

            // Query adjacent months in after current one is finished
            var now = new DateTime(year, month, 1);
            var prevMonth = now.AddMonths(-1);
            var nextMonth = now.AddMonths(1);
            var prevMonthTask = FetchEntriesForMonthAsync(prevMonth.Year, prevMonth.Month);
            var nextMonthTask = FetchEntriesForMonthAsync(nextMonth.Year, nextMonth.Month);

            await Task.WhenAll(prevMonthTask, nextMonthTask);
        }

        private async Task FetchEntriesForMonthAsync(int year, int month)
        {
            // Do nothing if we have previously fetched that data
            if (FetchedMonths.Any(x => x.year == year && x.month == month)) return;

            var fetchedData = await Http.GetFromJsonAsync<Entry[]>($"Entries?year={year}&month={month}");
            foreach (var entry in fetchedData)
            {
                Entries.Add(entry.Date.Date, entry);
            }

            // Mark as fetched
            FetchedMonths.Add((year, month));
        }
    }
}