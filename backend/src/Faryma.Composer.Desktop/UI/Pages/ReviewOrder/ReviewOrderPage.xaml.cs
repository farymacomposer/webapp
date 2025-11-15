using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ReviewOrderPage : Page
    {
        private ReviewOrderPageVM ViewModel { get; } = App.GetService<ReviewOrderPageVM>();

        public ReviewOrderPage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();
        }

        //private void List_GotFocus(object _, RoutedEventArgs __) => Control1.StartBringIntoView();
    }
}