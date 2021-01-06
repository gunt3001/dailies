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
        private List<(int year, int month)> FetchingMonths { get; set; } = new List<(int year, int month)>();

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

            // Delay if data is being fetched
            while (FetchingMonths.Any(x => x.month == date.Month && x.year == date.Year))
            {
                await Task.Delay(500);
            }

            // Fetch and return
            await FetchEntriesForMonthAsync(date.Year, date.Month);
            return GetEntryOrNull(date);
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

        public async Task<Entry> GetRandomEntryAsync()
        {
            var randomEntry = await Http.GetFromJsonAsync<Entry>($"Entries/random");
            return randomEntry;
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
            // Do nothing if we have previously fetched or is fetching that month's data
            if (FetchedMonths.Any(x => x.year == year && x.month == month)
            || FetchingMonths.Any(x => x.year == year && x.month == month))
            {
                return;
            }

            // Mark as fetching
            // Note: there's unlikely to be a race condition here since WASM is single-threaded.
            var marker = (year, month);
            FetchingMonths.Add(marker);

            var fetchedData = await Http.GetFromJsonAsync<Entry[]>($"Entries?year={year}&month={month}");
            foreach (var entry in fetchedData)
            {
                Entries[entry.Date.Date] = entry;
            }

            // Remove fetching flag, set fetched flag.
            FetchedMonths.Add(marker);
            FetchingMonths.Remove(marker);
        }

        public async Task<bool> AddOrUpdateEntryAsync(Entry entry, bool updateExisting)
        {
            var uri = "Entries";
            if (updateExisting) uri += "?updateExisting=true";
            try
            {
                var putResponse = await Http.PutAsJsonAsync(uri, entry);
                if (!putResponse.IsSuccessStatusCode) return false;
                // Update local data
                Entries[entry.Date] = entry;
                return true;
            }
            // Since PutAsJsonAsync Method calls Javascript fetch, 
            // when it fails, the exception thrown is a generic one
            catch (Exception)
            {
                return false;
            }
        }
    }
}