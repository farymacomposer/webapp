using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Api.ComposerStream;
using Faryma.Composer.Desktop.Api.Shared.Dto;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.ViewModels;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ComposerStreamDaySlotEditorDialogVM(
        ComposerStreamHttpClient composerStreamHttpClient,
        MessageService messageService,
        DialogService dialogService) : DialogVM(dialogService)
    {
        public ComposerStreamType[] StreamTypes { get; } =
        [
            ComposerStreamType.Donation,
            ComposerStreamType.Debt,
            ComposerStreamType.Charity,
        ];

        [ObservableProperty]
        public partial DateOnly Date { get; set; }

        [ObservableProperty]
        public partial ComposerStreamType StreamType { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasStream))]
        [NotifyPropertyChangedFor(nameof(NoStream))]
        [NotifyCanExecuteChangedFor(nameof(CreateStreamCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartStreamCommand))]
        [NotifyCanExecuteChangedFor(nameof(CompleteStreamCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelStreamCommand))]
        public partial ComposerStreamVM? Stream { get; set; }

        public bool HasStream => Stream is not null;
        public bool NoStream => Stream is null;

        public override Task OnNavigatedTo(object? parameter)
        {
            var daySlot = (ComposerStreamDaySlotVM)parameter!;
            Date = daySlot.Date;
            Stream = daySlot.Stream;
            StreamType = ComposerStreamType.Donation;

            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(NoStream))]
        private Task CreateStream() => UpdateStream(composerStreamHttpClient.Create(Date, StreamType));

        [RelayCommand(CanExecute = nameof(HasStream))]
        private Task StartStream() => UpdateStream(composerStreamHttpClient.Start(Stream!.Id));

        [RelayCommand(CanExecute = nameof(HasStream))]
        private Task CompleteStream() => UpdateStream(composerStreamHttpClient.Complete(Stream!.Id));

        [RelayCommand(CanExecute = nameof(HasStream))]
        private Task CancelStream() => UpdateStream(composerStreamHttpClient.Cancel(Stream!.Id));

        private Task UpdateStream(Task<ComposerStreamDto> task) => messageService.HandleException(async () =>
        {
            ComposerStreamDto dto = await task;
            Stream = new ComposerStreamVM(dto);
        });
    }
}