using Faryma.Composer.Desktop.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNavigation(this IServiceCollection services) => services
            .AddSingleton<DialogService>()
            .AddSingleton<MessageService>()
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowVM>();

        public static IServiceCollection AddPages(this IServiceCollection services) => services
            .AddSingleton<OrderQueuePageVM>()
            .AddSingleton<ReviewOrderPageVM>();

        public static IServiceCollection AddDialog<TDialog, TViewModel>(this IServiceCollection services)
            where TDialog : UserControl
            where TViewModel : DialogVM => services
                .AddSingleton<TDialog>()
                .AddSingleton<TViewModel>();
    }
}