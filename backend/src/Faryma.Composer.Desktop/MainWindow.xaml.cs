using System.Collections;
using Faryma.Composer.Desktop.UI.OrderQueue;
using Faryma.Composer.Desktop.ViewModels;
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
                Icon = "\xE8CB", // Sort
                PageType = typeof(OrderQueuePage),
            },
            //new()
            //{
            //    Index = 1,
            //    Title = "Транзакции",
            //    Icon = "\xE71C", // Filter
            //    PageType = typeof(TransactionsPage),
            //},
            //new()
            //{
            //    Index = 2,
            //    Title = "Счета",
            //    Icon = "\xE8C7", // PaymentCard
            //    PageType = typeof(AccountsPage),
            //},
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