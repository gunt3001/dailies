using System;
using System.Threading.Tasks;
using dailies.Client.Models;
using Microsoft.AspNetCore.Components;

namespace dailies.Client.Pages
{
    public partial class Calendar
    {
        [Inject]
        private EntriesManager EntriesManager { get; set; }

        private int CurrentYear { get; set; }
        private int _currentMonth = 0;
        private int CurrentMonth
        {
            get
            {
                return _currentMonth;
            }
            set
            {
                _currentMonth = value;
                if (_currentMonth == 13)
                {
                    CurrentYear += 1;
                    _currentMonth = 1;
                }
                else if (_currentMonth == 0 && CurrentYear != 0)
                {
                    CurrentYear -= 1;
                    _currentMonth = 12;
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            CurrentYear = DateTime.Today.Year;
            CurrentMonth = DateTime.Today.Month;
            await TriggerCalendarUpdateAsync();
        }

        private async Task TriggerCalendarUpdateAsync()
        {
            if (CurrentYear == 0 || CurrentMonth == 0) return;
            await EntriesManager.FetchEntriesToDisplayForMonthAsync(CurrentYear, CurrentMonth);
            StateHasChanged();
        }

        private async Task SetCurrentMonthAsync(object month)
        {
            if (month == null || !(int.TryParse((string)month, out var parsedMonth)))
            {
                return;
            }
            CurrentMonth = parsedMonth;
            await TriggerCalendarUpdateAsync();
        }

        private async Task SetCurrentYearAsync(object year)
        {
            if (year == null || !(int.TryParse((string)year, out var parsedYear)))
            {
                return;
            }
            CurrentYear = parsedYear;
            await TriggerCalendarUpdateAsync();
        }

        private async Task DecrementMonthAsync()
        {
            CurrentMonth--;
            await TriggerCalendarUpdateAsync();
        }

        private async Task IncrementMonthAsync()
        {
            CurrentMonth++;
            await TriggerCalendarUpdateAsync();
        }
    }
}