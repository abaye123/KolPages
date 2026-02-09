namespace KolPages.Models
{
    /// <summary>
    /// Represents the keyboard input mode
    /// </summary>
    public enum KeyboardMode
    {
        /// <summary>
        /// Keyboard Handler mode - uses JavaScript injection
        /// </summary>
        KeyboardHandler,
        
        /// <summary>
        /// SendKeys mode - uses SendKeys.Send
        /// </summary>
        SendKeys
    }
}
