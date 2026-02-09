using System;
using System.Windows;
using System.IO;
using KolPages.ViewModels;
using KolPages.Services;

namespace KolPages;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IWebNavigationService _navigationService;
    private readonly IDomManipulationService _domService;
    private readonly string _webViewUserDataFolder;

    public MainWindow(MainViewModel viewModel, IWebNavigationService navigationService, IDomManipulationService domService)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        _navigationService = navigationService;
        _domService = domService;

        _webViewUserDataFolder = Path.Combine(Path.GetTempPath(), "KolPages", "WebView2", Guid.NewGuid().ToString("N"));
        webView.CreationProperties = new Microsoft.Web.WebView2.Wpf.CoreWebView2CreationProperties
        {
            UserDataFolder = _webViewUserDataFolder
        };
        
        DataContext = _viewModel;
        
        // Initialize WebView2
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Ensure WebView2 is initialized
        await webView.EnsureCoreWebView2Async();

        // Disable right-click context menu
        if (webView.CoreWebView2 != null)
        {
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        }
        
        // Initialize services with WebView2
        if (_navigationService is WebNavigationService webNavService)
        {
            webNavService.Initialize(webView);
        }
        
        if (_domService is DomManipulationService domManipService)
        {
            domManipService.Initialize(webView);
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            webView.Dispose();
        }
        catch
        {
            // Ignore disposal errors during shutdown
        }

        try
        {
            if (Directory.Exists(_webViewUserDataFolder))
            {
                Directory.Delete(_webViewUserDataFolder, true);
            }
        }
        catch
        {
            // Ignore cleanup errors to allow app to close
        }
    }
}
