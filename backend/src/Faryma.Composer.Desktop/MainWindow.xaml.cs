using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.Navigation.Message;
using Faryma.Composer.Desktop.UI.OrderQueueFeature;
using Faryma.Composer.Desktop.UI.ReviewOrderFeature;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop
{
    public sealed partial class MainWindow : Window
    {
        private PageViewModel[] Pages { get; } =
        [
            new()
            {
                Title = "Заказы",
                Icon = "\xE71D",
                PageType = typeof(ReviewOrderPage),
            },
            new()
            {
                Title = "Тест очереди",
                Icon = "\xE71D",
                PageType = typeof(OrderQueuePage),
            },
        ];

        public MainWindow(MessageService messageService)
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(titleBar);

            messageService.SetFrame(MessageFrame);
        }

        private void NavigationViewLoaded(object sender, RoutedEventArgs _)
        {
            if (sender is NavigationView navigationView)
            {
                navigationView.SelectedItem = ((IList)navigationView.MenuItemsSource)[0];
            }
        }

        private void NavigationViewSelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is PageViewModel page)
            {
                PageFrame.Navigate(page.PageType);
            }
        }

        private void TitleBar_PaneToggleRequested(TitleBar _, object __) => navView.IsPaneOpen = !navView.IsPaneOpen;
    }

    public sealed partial class PageViewModel : ObservableObject
    {
        public string Icon { get; init; } = null!;
        public Type PageType { get; init; } = null!;
        public string Title { get; init; } = null!;
    }
}