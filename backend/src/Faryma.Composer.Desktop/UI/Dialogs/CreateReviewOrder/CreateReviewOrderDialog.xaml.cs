using Faryma.Composer.Desktop.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class CreateReviewOrderDialog : UserControl, IDialogControl<CreateReviewOrderDialogVM>
    {
        public CreateReviewOrderDialogVM ViewModel { get; } = App.GetService<CreateReviewOrderDialogVM>();

        public CreateReviewOrderDialog()
        {
            InitializeComponent();
        }
    }
}