namespace KolPages.Models
{
    /// <summary>
    /// Application configuration loaded from appsettings.json
    /// </summary>
    public class AppConfiguration
    {
        public List<WebsiteInfo> Websites { get; set; } = new();
        public WindowSettings Window { get; set; } = new();
        public KeyboardSettings Keyboard { get; set; } = new();
    }

    public class WindowSettings
    {
        public int CollapsedWidth { get; set; } = 75;
        public int CollapsedHeight { get; set; } = 75;
        public bool AlwaysOnTop { get; set; } = true;
        public string ButtonCorner { get; set; } = "TopLeft"; // TopLeft, TopRight, BottomLeft, BottomRight
    }

    public class KeyboardSettings
    {
        public KeyboardMode DefaultMode { get; set; } = KeyboardMode.KeyboardHandler;
        public bool ShowVirtualKeyboard { get; set; } = true;
    }
}
