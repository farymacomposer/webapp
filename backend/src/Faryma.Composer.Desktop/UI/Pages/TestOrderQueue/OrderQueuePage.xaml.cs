using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class OrderQueuePage : Page
    {
        private OrderQueuePageVM ViewModel { get; } = App.GetService<OrderQueuePageVM>();

        public OrderQueuePage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();
        }
    }
}