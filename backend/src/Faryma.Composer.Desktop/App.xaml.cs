using Faryma.Composer.Desktop.Services.OrderQueueFeature;
using Faryma.Composer.Desktop.UI.OrderQueueFeature;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Faryma.Composer.Desktop
{
    public partial class App : Application
    {
        private static readonly ServiceProvider _services;

        static App()
        {
            ServiceCollection services = new();

            services.AddHttpClient("Faryma.Composer.Api", client => client.BaseAddress = new Uri("https://api.example.com"));

            services.AddSingleton<OrderQueueService>();

            services.AddSingleton<OrderQueuePageVM>();

            _services = services.BuildServiceProvider();
        }

        public App()
        {
            InitializeComponent();
        }

        public static T GetService<T>() where T : notnull => _services.GetRequiredService<T>();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow window = new();
            window.Activate();
        }
    }
}