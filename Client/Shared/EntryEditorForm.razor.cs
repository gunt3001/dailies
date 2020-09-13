using System;
using dailies.Client.Models;
using dailies.Shared;
using Microsoft.AspNetCore.Components;

namespace dailies.Client.Shared
{
    public partial class EntryEditorForm
    {
        [Inject]
        private DateUtilities DateUtilities { get; set; }

        [Inject]
        private EntriesManager EntriesManager { get; set; }

        private DateTime? _date = DateTime.Today;
        [Parameter]
        public DateTime? Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                if (value != null)
                {
                    // On date change, load entry
                    var entry = EntriesManager.GetEntryAsync(value.Value).Result;
                    SetFormFromEntry(entry);
                }
            }
        }

        // Entry details (excluding the date)
        private string Content { get; set; }
        private string Keyword { get; set; }
        private string Mood { get; set; }
        private string Remarks { get; set; }

        private int CurrentContentLength => Content?.Length ?? 0;
        private bool IsContentBeyondMaxLength => CurrentContentLength > MaxContentLength;

        [Parameter]
        public int MaxContentLength { get; set; } = 120;

        private string RelativeDateString => Date == null ? null : DateUtilities.GetRelativeDateFromToday(Date.Value);

        private void MoveDate(int offset)
        {
            if (Date == null) return;
            Date = Date.Value.AddDays(offset);
        }

        private void SetFormFromEntry(Entry entry)
        {
            Content = entry?.Content ?? "";
            Keyword = entry?.Keyword ?? "";
            Mood = entry?.Mood ?? "";
            Remarks = entry?.Remarks ?? "";
        }
    }
}