using System;

namespace dailies.Shared
{
    public class Entry
    {
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string Keyword { get; set; }
        public string Mood { get; set; }
        public string Remarks { get; set; }

        // Replace the contents of this entry with another, keeping the date
        public void LoadContentsFrom(Entry entry)
        {
            Content = entry.Content;
            Keyword = entry.Keyword;
            Mood = entry.Mood;
            Remarks = entry.Remarks;
        }
    }
}
