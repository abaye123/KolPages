namespace KolPages.Models
{
    /// <summary>
    /// Represents information about a website that can be navigated to
    /// </summary>
    public class WebsiteInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public string TimerName { get; set; } = string.Empty;
        
        /// <summary>
        /// DOM element IDs to remove when this website loads
        /// </summary>
        public List<string> ElementsToRemove { get; set; } = new();

        /// <summary>
        /// DOM selectors to remove when this website loads
        /// </summary>
        public List<string> ElementsToRemoveSelectors { get; set; } = new();
    }
}
