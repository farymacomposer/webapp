using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Api.ComposerStream;
using Faryma.Composer.Desktop.Api.Shared.Dto;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Utils;
using Faryma.Composer.Desktop.ViewModels;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ComposerStreamPageVM(
        ComposerStreamHttpClient composerStreamHttpClient,
        DialogService dialogService,
        MessageService messageService
        ) : ObservableObject
    {
        [ObservableProperty]
        public partial DateOnly CurrentMonth { get; set; }

        public ObservableCollection<StreamDaySlotVM> Days { get; } = [];

        public Task Initialize() => GoToCurrentMonth();

        private async Task LoadMonth(DateOnly month)
        {
            const int gridSize = 42;

            CurrentMonth = month.GetFirstDayOfMonth();
            Days.Clear();

            await messageService.HandleException(async () =>
            {
                DateOnly dateFrom = CurrentMonth.StartOfWeek(DayOfWeek.Monday);
                DateOnly dateTo = dateFrom.AddDays(gridSize - 1);

                IEnumerable<ComposerStreamDto> streams = await composerStreamHttpClient.Find(dateFrom, dateTo);

                for (int i = 0; i < gridSize; i++)
                {
                    DateOnly date = dateFrom.AddDays(i);
                    ComposerStreamDto? dto = streams.FirstOrDefault(x => x.EventDate == date);

                    Days.Add(new StreamDaySlotVM
                    {
                        Date = date,
                        IsCurrentMonth = date.Month == CurrentMonth.Month,
                        Stream = (dto is null) ? null : new ComposerStreamVM(dto),
                    });
                }
            });
        }

        [RelayCommand]
        private Task GoToCurrentMonth() => LoadMonth(DateOnly.FromDateTime(DateTime.Now));

        [RelayCommand]
        private Task PrevMonth() => LoadMonth(CurrentMonth.AddMonths(-1));

        [RelayCommand]
        private Task NextMonth() => LoadMonth(CurrentMonth.AddMonths(1));
    }
}