using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Api.ComposerStream;
using Faryma.Composer.Desktop.Api.Shared.Dto;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Utils;
using Faryma.Composer.Desktop.ViewModels;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ComposerStreamPageVM(
        ComposerStreamHttpClient composerStreamHttpClient,
        MessageService messageService
        ) : ObservableObject
    {
        private ComposerStreamDaySlotVM? _selectedDaySlot;

        public ComposerStreamType[] StreamTypes { get; } =
        [
            ComposerStreamType.Donation,
            ComposerStreamType.Debt,
            ComposerStreamType.Charity,
        ];

        public ObservableCollection<ComposerStreamDaySlotVM> Days { get; } = [];

        [ObservableProperty]
        public partial DateOnly CurrentMonth { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateStreamCommand))]
        public partial DateOnly? SelectedDate { get; set; }

        [ObservableProperty]
        public partial ComposerStreamType SelectedStreamType { get; set; } = ComposerStreamType.Donation;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateStreamCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartStreamCommand))]
        [NotifyCanExecuteChangedFor(nameof(CompleteStreamCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelStreamCommand))]
        public partial ComposerStreamVM? SelectedStream { get; set; }

        public bool CanCreateStream => SelectedStream is null && SelectedDate >= DateOnly.FromDateTime(DateTime.UtcNow);
        public bool CanStartStream => SelectedStream?.Status == ComposerStreamStatus.Planned;
        public bool CanCompleteStream => SelectedStream?.Status == ComposerStreamStatus.Live;
        public bool CanCancelStream => SelectedStream?.Status == ComposerStreamStatus.Planned;

        public Task Initialize() => GoToCurrentMonth();

        private async Task LoadMonth(DateOnly month)
        {
            CurrentMonth = month.GetFirstDayOfMonth();
            Days.Clear();
            SelectedDate = null;
            SelectedStream = null;
            _selectedDaySlot = null;

            await messageService.HandleException(async () =>
            {
                DateOnly dateFrom = CurrentMonth.StartOfWeek(DayOfWeek.Monday);
                DateOnly dateTo = dateFrom.AddDays(41);

                IEnumerable<ComposerStreamDto> streams = await composerStreamHttpClient.Find(dateFrom, dateTo);

                for (DateOnly date = dateFrom; date <= dateTo; date = date.AddDays(1))
                {
                    ComposerStreamDto? dto = streams.FirstOrDefault(x => x.EventDate == date);

                    Days.Add(new ComposerStreamDaySlotVM
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

        [RelayCommand]
        private void SelectDaySlot(ComposerStreamDaySlotVM daySlot)
        {
            _selectedDaySlot?.IsSelected = false;
            _selectedDaySlot = daySlot;
            _selectedDaySlot.IsSelected = true;

            SelectedDate = daySlot.Date;
            SelectedStream = daySlot.Stream;
            SelectedStreamType = ComposerStreamType.Donation;
        }

        [RelayCommand(CanExecute = nameof(CanCreateStream))]
        private Task CreateStream() => UpdateStream(composerStreamHttpClient.Create(SelectedDate!.Value, SelectedStreamType));

        [RelayCommand(CanExecute = nameof(CanStartStream))]
        private Task StartStream() => UpdateStream(composerStreamHttpClient.Start(SelectedStream!.Id));

        [RelayCommand(CanExecute = nameof(CanCompleteStream))]
        private Task CompleteStream() => UpdateStream(composerStreamHttpClient.Complete(SelectedStream!.Id));

        [RelayCommand(CanExecute = nameof(CanCancelStream))]
        private Task CancelStream() => UpdateStream(composerStreamHttpClient.Cancel(SelectedStream!.Id));

        private Task UpdateStream(Task<ComposerStreamDto> task) => messageService.HandleException(async () =>
        {
            ComposerStreamDto dto = await task;
            SelectedStream = new ComposerStreamVM(dto);
            if (Days.FirstOrDefault(x => x.Date == dto.EventDate) is ComposerStreamDaySlotVM daySlot)
            {
                daySlot.Stream = SelectedStream;
            }
        });
    }
}