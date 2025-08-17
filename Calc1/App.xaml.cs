using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernCalculator.Services;
using ModernCalculator.ViewModels;
using System;
using System.Windows;

namespace ModernCalculator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Set up dependency injection and services
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Services
                    services.AddSingleton<ICalculationEngine, AdvancedCalculationEngine>();
                    services.AddSingleton<IHistoryService, HistoryService>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    services.AddSingleton<ISettingsService, SettingsService>();

                    // ViewModels
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<ScientificCalculatorViewModel>();
                    services.AddTransient<ProgrammerCalculatorViewModel>();
                    services.AddTransient<HistoryViewModel>();
                })
                .Build();

            // Start the host
            _host.Start();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }

        public static T GetService<T>()
            where T : class
        {
            if ((Current as App)?._host?.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }
    }
}