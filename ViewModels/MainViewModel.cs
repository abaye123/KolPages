using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KolPages.Models;
using KolPages.Services;
using System.Collections.ObjectModel;

namespace KolPages.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IWebNavigationService _navigationService;
        private readonly IDomManipulationService _domService;
        private readonly AppConfiguration _config;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private double _windowWidth;

        [ObservableProperty]
        private double _windowHeight;

        [ObservableProperty]
        private KeyboardMode _currentKeyboardMode;

        [ObservableProperty]
        private bool _isKeyboardVisible;

        [ObservableProperty]
        private bool _isAboutVisible;

        [ObservableProperty]
        private bool _isLoadingVisible;

        [ObservableProperty]
        private bool _isBrowserVisible;

        [ObservableProperty]
        private double _windowLeft;

        [ObservableProperty]
        private double _windowTop;

        [ObservableProperty]
        private string _keyboardLayout;

        private const double ToolbarHeight = 92;

        public ObservableCollection<WebsiteInfo> Websites { get; }

        public MainViewModel(
            IWebNavigationService navigationService,
            IDomManipulationService domService,
            AppConfiguration config)
        {
            _navigationService = navigationService;
            _domService = domService;
            _config = config;

            // Initialize from configuration
            Websites = new ObservableCollection<WebsiteInfo>(config.Websites);
            _currentKeyboardMode = config.Keyboard.DefaultMode;
            _windowWidth = config.Window.CollapsedWidth;
            _windowHeight = config.Window.CollapsedHeight;
            _isExpanded = false;
            _isKeyboardVisible = false;
            _isBrowserVisible = false;
            _isAboutVisible = false;
            _isLoadingVisible = false;
            _keyboardLayout = "Hebrew";

            // Calculate initial window position based on button corner
            CalculateWindowPosition();
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;

            if (IsExpanded)
            {
                // Expand to full screen width and toolbar height
                WindowWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                WindowHeight = ToolbarHeight;

                IsBrowserVisible = false;
                IsKeyboardVisible = false;
                IsAboutVisible = false;
                IsLoadingVisible = false;

                // Adjust position to top-left when expanded
                WindowLeft = 0;
                WindowTop = 0;
            }
            else
            {
                // Collapse to small button
                WindowWidth = _config.Window.CollapsedWidth;
                WindowHeight = _config.Window.CollapsedHeight;

                IsBrowserVisible = false;
                IsKeyboardVisible = false;
                IsAboutVisible = false;
                IsLoadingVisible = false;

                // Recalculate position for collapsed state
                CalculateWindowPosition();
            }
        }

        private void CalculateWindowPosition()
        {
            var cornerPosition = _config.Window.ButtonCorner;
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            switch (cornerPosition)
            {
                case "TopRight":
                    WindowLeft = screenWidth - WindowWidth;
                    WindowTop = 0;
                    break;
                case "BottomLeft":
                    WindowLeft = 0;
                    WindowTop = screenHeight - WindowHeight;
                    break;
                case "BottomRight":
                    WindowLeft = screenWidth - WindowWidth;
                    WindowTop = screenHeight - WindowHeight;
                    break;
                case "TopLeft":
                default:
                    WindowLeft = 0;
                    WindowTop = 0;
                    break;
            }
        }

        [RelayCommand]
        private async Task NavigateToWebsite(WebsiteInfo website)
        {
            if (website == null) return;

            if (!IsExpanded)
            {
                ToggleExpand();
            }

            // Expand to full screen when a site is selected
            WindowHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            IsBrowserVisible = false;
            IsKeyboardVisible = false;
            IsAboutVisible = true;
            IsLoadingVisible = true;

            await _navigationService.NavigateToWebsiteAsync(website);
            IsAboutVisible = false;
            IsLoadingVisible = false;
            IsBrowserVisible = true;
        }

        [RelayCommand]
        private async Task InsertText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            await _domService.InsertTextAsync(text);
        }

        [RelayCommand]
        private async Task DeleteLastCharacter()
        {
            await _domService.DeleteLastCharacterAsync();
        }

        [RelayCommand]
        private async Task PressEnter()
        {
            await _domService.PressEnterAsync();
        }

        [RelayCommand]
        private void ToggleKeyboard()
        {
            if (!IsBrowserVisible)
            {
                return;
            }

            IsKeyboardVisible = !IsKeyboardVisible;
            // Height adjustment is automatic in expanded mode via the XAML layout
        }

        [RelayCommand]
        private void ToggleKeyboardMode()
        {
            CurrentKeyboardMode = CurrentKeyboardMode == KeyboardMode.KeyboardHandler
                ? KeyboardMode.SendKeys
                : KeyboardMode.KeyboardHandler;
        }

        [RelayCommand]
        private void SetKeyboardLayout(string layout)
        {
            if (string.IsNullOrWhiteSpace(layout)) return;
            KeyboardLayout = layout;
        }

        [RelayCommand]
        private async Task ShowAbout()
        {
            if (!IsExpanded)
            {
                ToggleExpand();
            }

            WindowHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            IsBrowserVisible = false;
            IsKeyboardVisible = false;
            IsAboutVisible = true;
            IsLoadingVisible = false;

            await Task.CompletedTask;
        }
    }
}
