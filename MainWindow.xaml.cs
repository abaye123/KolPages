using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
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

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        var newStyle = new IntPtr(exStyle.ToInt64() | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, newStyle);
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

    private const int GWL_EXSTYLE = -20;
    private const long WS_EX_TOOLWINDOW = 0x00000080L;
    private const long WS_EX_NOACTIVATE = 0x08000000L;

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }
}
