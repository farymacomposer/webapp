using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Faryma.Composer.Desktop.UI.ReviewOrderFeature
{
    public sealed partial class ReviewOrderPage : Page
    {
        private ReviewOrderPageVM ViewModel { get; } = App.GetService<ReviewOrderPageVM>();

        public ReviewOrderPage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();
        }
    }
}