using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.ViewModels
{
    public sealed partial class ComposerStreamDaySlotVM : ObservableObject
    {
        public DateOnly Date { get; init; }
        public bool IsCurrentMonth { get; init; }
        public ComposerStreamVM? Stream { get; init; }
        public bool HasStream => Stream is not null;
    }
}