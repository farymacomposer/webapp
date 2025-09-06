using System.Collections;
using Faryma.Composer.Desktop.Shared.ViewModels;
using Faryma.Composer.Desktop.UI.OrderQueueFeature;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Faryma.Composer.Desktop
{
    public sealed partial class MainWindow : Window
    {
        private PageViewModel? _currentPage;

        private PageViewModel[] Pages { get; } =
        [
            new()
            {
                Index = 0,
                Title = "Очередь",
                Icon = "\xE71D",
                PageType = typeof(OrderQueuePage),
            },
        ];

        public MainWindow()
        {
            InitializeComponent();
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
                if (_currentPage is null)
                {
                    PageFrame.Navigate(page.PageType);
                }
                else
                {
                    SlideNavigationTransitionEffect effect = (_currentPage.Index < page.Index)
                        ? SlideNavigationTransitionEffect.FromRight
                        : SlideNavigationTransitionEffect.FromLeft;

                    PageFrame.Navigate(page.PageType, null, new SlideNavigationTransitionInfo { Effect = effect });
                }

                _currentPage = page;
            }
        }
    }
}