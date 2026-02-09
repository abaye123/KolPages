using Microsoft.Web.WebView2.Wpf;

namespace KolPages.Services
{
    /// <summary>
    /// Interface for DOM manipulation operations
    /// </summary>
    public interface IDomManipulationService
    {
        /// <summary>
        /// Initialize the service with a WebView2 control
        /// </summary>
        void Initialize(WebView2 webView);

        /// <summary>
        /// Remove an element by its ID
        /// </summary>
        Task RemoveElementByIdAsync(string elementId);
        
        /// <summary>
        /// Remove multiple elements by their IDs
        /// </summary>
        Task RemoveElementsByIdsAsync(IEnumerable<string> elementIds);

        /// <summary>
        /// Remove elements by CSS selectors
        /// </summary>
        Task RemoveElementsBySelectorsAsync(IEnumerable<string> selectors);
        
        /// <summary>
        /// Remove elements by onclick attribute
        /// </summary>
        Task RemoveElementByOnClickAsync(string onClickValue);
        
        /// <summary>
        /// Remove elements by href attribute
        /// </summary>
        Task RemoveElementByHrefAsync(string href);
        
        /// <summary>
        /// Insert text into focused input field
        /// </summary>
        Task InsertTextAsync(string text);
        
        /// <summary>
        /// Delete last character from focused input field
        /// </summary>
        Task DeleteLastCharacterAsync();
        
        /// <summary>
        /// Trigger Enter key press on focused element
        /// </summary>
        Task PressEnterAsync();
    }
}
