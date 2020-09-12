using System;
using System.Collections.Generic;
using System.Linq;
using dailies.Shared;

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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool UpdateEntry(Entry entry)
        {
            throw new NotImplementedException();
        }
    }
}
