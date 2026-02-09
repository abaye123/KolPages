using KolPages.Models;
using Microsoft.Web.WebView2.Wpf;
using System.Threading;

namespace KolPages.Services
{
    /// <summary>
    /// Implementation of web navigation service using WebView2
    /// </summary>
    public class WebNavigationService : IWebNavigationService
    {
        private WebView2? _webView;
        private readonly IDomManipulationService _domService;
        private WebsiteInfo? _currentWebsite;
        private readonly SemaphoreSlim _removalLock = new(1, 1);
        private bool _navigationHooked;
        private Timer? _toraRemovalTimer;

        public WebNavigationService(IDomManipulationService domService)
        {
            _domService = domService;
        }

        /// <summary>
        /// Initialize the service with a WebView2 control
        /// </summary>
        public void Initialize(WebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));

            if (!_navigationHooked)
            {
                _webView.NavigationCompleted += WebView_NavigationCompleted;
                _navigationHooked = true;
            }
        }

        public async Task NavigateAsync(string url)
        {
            if (_webView == null)
                throw new InvalidOperationException("WebView2 not initialized");

            // Add timestamp to prevent caching for http/https only
            var updatedUrl = url;
            if (Uri.TryCreate(url, UriKind.Absolute, out var parsedUri)
                && (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
            {
                var timestamp = DateTime.Now.Ticks;
                updatedUrl = url.Contains('?') ? $"{url}&timestamp={timestamp}" : $"{url}?timestamp={timestamp}";
            }
            
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
            {
                _webView.NavigationCompleted -= Handler;
                tcs.TrySetResult(args.IsSuccess);
            }

            _webView.NavigationCompleted += Handler;
            _webView.Source = new Uri(updatedUrl);

            await tcs.Task;
        }

        public async Task NavigateToWebsiteAsync(WebsiteInfo website)
        {
            _currentWebsite = website;
            await NavigateAsync(website.Url);

            await ApplyDomRemovalsWithRetriesAsync(website, 3, 400);

            ConfigureToraRemovalTimer(website);
        }

        private void WebView_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_webView == null || _currentWebsite == null)
            {
                return;
            }

            var currentUrl = _webView.Source?.ToString() ?? string.Empty;
            if (!IsSameHost(_currentWebsite.Url, currentUrl))
            {
                StopToraRemovalTimer();
                return;
            }

            _ = ApplyDomRemovalsWithRetriesAsync(_currentWebsite, 3, 400);
            ConfigureToraRemovalTimer(_currentWebsite);
        }

        private void ConfigureToraRemovalTimer(WebsiteInfo website)
        {
            if (!IsToraWebsite(website))
            {
                StopToraRemovalTimer();
                return;
            }

            if (_toraRemovalTimer != null)
            {
                return;
            }

            _toraRemovalTimer = new Timer(async _ =>
            {
                if (_currentWebsite == null || _webView == null)
                {
                    return;
                }

                await _webView.Dispatcher.InvokeAsync(async () =>
                {
                    var currentUrl = _webView.Source?.ToString() ?? string.Empty;
                    if (!IsSameHost(_currentWebsite.Url, currentUrl))
                    {
                        return;
                    }

                    await ApplyDomRemovalsWithRetriesAsync(_currentWebsite, 1, 0);
                });
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void StopToraRemovalTimer()
        {
            _toraRemovalTimer?.Dispose();
            _toraRemovalTimer = null;
        }

        private static bool IsToraWebsite(WebsiteInfo website)
        {
            return string.Equals(website.Name, "Tora", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ApplyDomRemovalsWithRetriesAsync(WebsiteInfo website, int attempts, int delayMs)
        {
            if (website.ElementsToRemove.Count == 0 && website.ElementsToRemoveSelectors.Count == 0)
            {
                return;
            }

            await _removalLock.WaitAsync();
            try
            {
                for (var i = 0; i < attempts; i++)
                {
                    await Task.Delay(delayMs);

                    if (website.ElementsToRemove.Count > 0)
                    {
                        await _domService.RemoveElementsByIdsAsync(website.ElementsToRemove);
                    }

                    if (website.ElementsToRemoveSelectors.Count > 0)
                    {
                        await _domService.RemoveElementsBySelectorsAsync(website.ElementsToRemoveSelectors);
                    }
                }
            }
            finally
            {
                _removalLock.Release();
            }
        }

        private static bool IsSameHost(string baseUrl, string currentUrl)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                return false;
            }

            if (!Uri.TryCreate(currentUrl, UriKind.Absolute, out var currentUri))
            {
                return false;
            }

            return string.Equals(baseUri.Host, currentUri.Host, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExecuteScriptAsync(string script)
        {
            if (_webView == null)
                throw new InvalidOperationException("WebView2 not initialized");

            try
            {
                return await _webView.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                System.Diagnostics.Debug.WriteLine($"Script execution failed: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task GoBackAsync()
        {
            if (_webView == null)
                throw new InvalidOperationException("WebView2 not initialized");

            if (_webView.CanGoBack)
            {
                _webView.GoBack();
            }
            await Task.CompletedTask;
        }

        public async Task ReloadAsync()
        {
            if (_webView == null)
                throw new InvalidOperationException("WebView2 not initialized");

            _webView.Reload();
            await Task.CompletedTask;
        }
    }
}
