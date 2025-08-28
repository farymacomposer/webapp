using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI.OrderQueueFeature
{
    [TemplateVisualState(Name = "Entered", GroupName = "PointerStates")]
    [TemplateVisualState(Name = "Exited", GroupName = "PointerStates")]
    public sealed partial class ReviewOrderControl : UserControl
    {
        public ReviewOrderControl()
        {
            InitializeComponent();

            PointerEntered += (_, _) => GoToState("Entered");
            PointerExited += (_, _) => GoToState("Exited");

            GoToState("Exited");
        }

        private void GoToState(string name) => VisualStateManager.GoToState(this, name, false);
    }
}