using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ComposerStreamPage : Page
    {
        private ComposerStreamPageVM ViewModel { get; } = App.GetService<ComposerStreamPageVM>();

        public ComposerStreamPage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();
        }
    }
}
