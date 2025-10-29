using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed partial class PageVM : ObservableObject
    {
        public required string Icon { get; init; }
        public required Type PageType { get; init; }
        public required string Title { get; init; }
    }
}