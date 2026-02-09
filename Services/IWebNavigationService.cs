using KolPages.Models;

namespace KolPages.Services
{
    /// <summary>
    /// Service for web navigation and WebView2 management
    /// </summary>
    public interface IWebNavigationService
    {
        /// <summary>
        /// Navigate to a specific URL
        /// </summary>
        Task NavigateAsync(string url);
        
        /// <summary>
        /// Navigate to a website with automatic DOM manipulation
        /// </summary>
        Task NavigateToWebsiteAsync(WebsiteInfo website);
        
        /// <summary>
        /// Execute JavaScript in the current page
        /// </summary>
        Task<string> ExecuteScriptAsync(string script);
        
        /// <summary>
        /// Go back to the previous page
        /// </summary>
        Task GoBackAsync();
        
        /// <summary>
        /// Reload the current page
        /// </summary>
        Task ReloadAsync();
    }
}
