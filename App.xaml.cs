using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KolPages.Models;
using KolPages.Services;
using KolPages.ViewModels;

namespace KolPages;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Load app configuration
        var appConfig = new AppConfiguration();
        configuration.Bind(appConfig);

        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, appConfig);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services, AppConfiguration appConfig)
    {
        // Register configuration
        services.AddSingleton(appConfig);

        // Register services
        services.AddSingleton<IDomManipulationService, DomManipulationService>();
        services.AddSingleton<IWebNavigationService, WebNavigationService>();

        // Register ViewModels
        services.AddSingleton<MainViewModel>();

        // Register Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
