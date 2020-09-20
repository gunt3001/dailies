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

        private int _currentYear = 0;
        private int currentYear
        {
            get
            {
                return _currentYear;
            }
            set
            {
                _currentYear = value;
                TriggerCalendarUpdateAsync();
            }
        }
        private int _currentMonth = 0;
        private int currentMonth
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
                    _currentYear += 1;
                    _currentMonth = 1;
                }
                else if (_currentMonth == 0 && _currentYear != 0)
                {
                    _currentYear -= 1;
                    _currentMonth = 12;
                }
                TriggerCalendarUpdateAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            currentYear = DateTime.Today.Year;
            currentMonth = DateTime.Today.Month;
        }

        private async Task TriggerCalendarUpdateAsync()
        {
            if (currentYear == 0 || currentMonth == 0) return;
            await EntriesManager.FetchEntriesToDisplayForMonthAsync(currentYear, currentMonth);
            StateHasChanged();
        }

    }
}