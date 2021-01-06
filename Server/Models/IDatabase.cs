using System;
using System.Collections.Generic;
using dailies.Shared;

namespace dailies.Server.Models
{
    public interface IDatabase
    {
        Entry GetEntry(DateTime date);
        Entry GetRandomEntry();
        IEnumerable<Entry> GetEntries(DateTime startDate, DateTime endDate);
        IEnumerable<Entry> GetEntries(int year, int month);
        AddEntryResult AddEntry(Entry newEntry);
        bool UpdateEntry(Entry entry);
    }
}
