using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Faryma.Composer.Desktop
{
    public partial class App : Application
    {
        private static readonly IServiceProvider _services;

        static App()
        {
            _services = new ServiceCollection()
                .BuildServiceProvider();
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