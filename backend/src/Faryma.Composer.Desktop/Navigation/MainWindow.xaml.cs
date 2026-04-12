using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed partial class MainWindow : Window
    {
        private MainWindowVM ViewModel { get; }

        public MainWindow(MainWindowVM viewModel, DialogService dialogService, MessageService messageService)
        {
            ViewModel = viewModel;

            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarControl);

            dialogService.SetFrame(DialogFrame);
            messageService.SetFrame(MessageFrame);
        }

        private void NavigationViewLoaded(object sender, RoutedEventArgs _)
        {
            if (sender is NavigationView navigationView && navigationView.MenuItemsSource is IList menuItems && menuItems.Count > 0)
            {
                navigationView.SelectedItem = menuItems[0];
            }
        }

        private void NavigationViewSelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is PageVM page)
            {
                PageFrame.Navigate(page.PageType);
            }
        }

        private void TitleBar_PaneToggleRequested(TitleBar _, object __) => NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
    }
}