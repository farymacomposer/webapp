using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Faryma.Composer.Desktop.UI.OrderQueueFeature
{
    public sealed partial class OrderQueuePage : Page
    {
        private OrderQueuePageVM ViewModel { get; } = App.GetService<OrderQueuePageVM>();

        public OrderQueuePage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();

            ViewModel.Page = this;
        }

        public async Task ShowDialog(string message)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }
}