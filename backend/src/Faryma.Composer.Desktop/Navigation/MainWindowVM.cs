using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Faryma.Composer.Desktop.Messages.Commands;
using Faryma.Composer.Desktop.UI;
using Microsoft.UI.Xaml;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed partial class MainWindowVM : ObservableObject,
        IRecipient<ShowProgressCommand>,
        IRecipient<HideProgressCommand>
    {
        [ObservableProperty]
        public partial bool ProgressIsActive { get; set; }

        [ObservableProperty]
        public partial Visibility ProgressVisibility { get; set; } = Visibility.Collapsed;

        public PageVM[] Pages { get; } =
        [
            new()
            {
                Title = "Заказы",
                Icon = "\xE71D",
                PageType = typeof(ReviewOrderPage),
            },
            new()
            {
                Title = "Расписание",
                Icon = "\xE71D",
                PageType = typeof(ComposerStreamPage),
            },
            new()
            {
                Title = "Тест очереди",
                Icon = "\xE71D",
                PageType = typeof(OrderQueuePage),
            },
        ];

        public MainWindowVM(IMessenger messenger)
        {
            messenger.RegisterAll(this);
        }

        public void Receive(ShowProgressCommand message)
        {
            ProgressIsActive = true;
            ProgressVisibility = Visibility.Visible;
        }

        public void Receive(HideProgressCommand message)
        {
            ProgressIsActive = false;
            ProgressVisibility = Visibility.Collapsed;
        }
    }
}