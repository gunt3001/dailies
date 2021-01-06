using System;
using System.Collections.Generic;
using System.Linq;
using dailies.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace dailies.Server.Models
{
    public class LocalSqliteDatabase : IDatabase
    {
        public EntriesContext Db { get; }

        public LocalSqliteDatabase(EntriesContext db)
        {
            Db = db;
        }

        public AddEntryResult AddEntry(Entry newEntry)
        {
            Db.Add(newEntry);
            try
            {
                Db.SaveChanges();
            }
            // Sqlite will throw with error code 19 on insert with duplicate key (same date)
            catch (DbUpdateException e) when ((e.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                return AddEntryResult.EntryExists;
            }

            return AddEntryResult.Success;
        }

        public IEnumerable<Entry> GetEntries(DateTime startDate, DateTime endDate)
        {
            return Db.Entries.Where(x => x.Date >= startDate.Date && x.Date <= endDate.Date);
        }

        public IEnumerable<Entry> GetEntries(int year, int month)
        {
            return GetEntries(new DateTime(year, month, 1),
                new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        }

        public Entry GetEntry(DateTime date)
        {
            return Db.Entries.Find(date);
        }

        public Entry GetRandomEntry()
        {
            var allValidEntries = Db.Entries.Where(x => x.Content != null && x.Content != "");
            var validEntriesCount = allValidEntries.Count();
            var randomEntryIndex = new Random().Next(0, validEntriesCount);
            return allValidEntries.Skip(randomEntryIndex).FirstOrDefault();
        }

        public bool UpdateEntry(Entry entry)
        {
            var existingEntry = GetEntry(entry.Date);
            if (existingEntry == null)
            {
                return false;
            }
            existingEntry.LoadContentsFrom(entry);
            Db.SaveChanges();
            return true;
        }
    }
}
