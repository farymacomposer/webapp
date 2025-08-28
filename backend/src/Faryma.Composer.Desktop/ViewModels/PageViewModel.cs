using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.ViewModels
{
    public sealed partial class PageViewModel : ObservableObject
    {
        public string Icon { get; init; } = null!;
        public Type PageType { get; init; } = null!;
        public string Title { get; init; } = null!;
        public int Index { get; init; }
    }
}