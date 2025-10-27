using Faryma.Composer.Desktop.Api.ComposerStream;
using Faryma.Composer.Desktop.Api.ReviewOrder;
using Faryma.Composer.Desktop.Services.OrderQueueFeature;
using Faryma.Composer.Desktop.UI.OrderQueueFeature;
using Faryma.Composer.Desktop.UI.ReviewOrderFeature;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
using Serilog.Events;

namespace Faryma.Composer.Desktop
{
    public partial class App : Application
    {
        public const string BaseAddress = "https://localhost:7166";

        private static readonly ServiceProvider _services;

        static App()
        {
            NativeMethods.AllocConsole();

            ServiceCollection services = new();

            services.AddLogging(builder => builder.AddSerilog(new LoggerConfiguration()
                .WriteTo
                .Console(LogEventLevel.Verbose, applyThemeToRedirectedOutput: true)
                .CreateLogger()));

            services.AddHttpClient<ReviewOrderHttpClient>(client => client.BaseAddress = new Uri(BaseAddress));
            services.AddHttpClient<ComposerStreamHttpClient>(client => client.BaseAddress = new Uri(BaseAddress));

            services.AddSingleton<OrderQueueService>();

            services.AddSingleton<OrderQueuePageVM>();
            services.AddSingleton<ReviewOrderPageVM>();

            _services = services.BuildServiceProvider();
        }

        public App()
        {
            InitializeComponent();
        }

        public static T GetService<T>() where T : notnull => _services.GetRequiredService<T>();

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await GetService<OrderQueueService>().Initialize();
            await GetService<OrderQueuePageVM>().Initialize();

            MainWindow window = new();
            window.Activate();
        }
    }
}