using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Services;
using Faryma.Composer.Desktop.UI;
using Faryma.Composer.Desktop.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNavigation(this IServiceCollection services) => services
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowVM>()
            .AddSingleton<DialogService>()
            .AddSingleton<MessageService>()
            .AddPages()
            .AddDialogs();

        public static IServiceCollection AddServices(this IServiceCollection services) => services
            .AddSingleton<OrderQueueService>()
            .AddSingleton<ValidationService>();

        private static IServiceCollection AddPages(this IServiceCollection services) => services
            .AddSingleton<ComposerStreamPageVM>()
            .AddSingleton<OrderQueuePageVM>()
            .AddSingleton<ReviewOrderPageVM>();

        private static IServiceCollection AddDialogs(this IServiceCollection services) => services
            .AddDialog<ComposerStreamDaySlotEditorDialog, ComposerStreamDaySlotEditorDialogVM>()
            .AddDialog<CreateReviewOrderDialog, CreateReviewOrderDialogVM>();

        private static IServiceCollection AddDialog<TDialog, TViewModel>(this IServiceCollection services)
            where TDialog : UserControl, IDialogControl<TViewModel>
            where TViewModel : DialogVM => services
                .AddSingleton<TDialog>()
                .AddSingleton<TViewModel>();
    }
}