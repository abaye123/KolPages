using Microsoft.Web.WebView2.Wpf;

namespace KolPages.Services
{
    /// <summary>
    /// Implementation of DOM manipulation service
    /// </summary>
    public class DomManipulationService : IDomManipulationService
    {
        private WebView2? _webView;

        public void Initialize(WebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        }

        private async Task<string> ExecuteScriptAsync(string script)
        {
            if (_webView == null)
                throw new InvalidOperationException("WebView2 not initialized");

            try
            {
                return await _webView.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Script execution failed: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task RemoveElementByIdAsync(string elementId)
        {
            var script = $@"
                var element = document.getElementById('{elementId}');
                if (element) {{
                    element.parentNode.removeChild(element);
                }}
            ";
            await ExecuteScriptAsync(script);
        }

        public async Task RemoveElementsByIdsAsync(IEnumerable<string> elementIds)
        {
            foreach (var elementId in elementIds)
            {
                await RemoveElementByIdAsync(elementId);
            }
        }

        public async Task RemoveElementsBySelectorsAsync(IEnumerable<string> selectors)
        {
            foreach (var selector in selectors)
            {
                if (string.IsNullOrWhiteSpace(selector))
                {
                    continue;
                }

                var script = $@"
                    var elements = document.querySelectorAll('{selector}');
                    for (var i = 0; i < elements.length; i++) {{
                        var element = elements[i];
                        if (element && element.parentNode) {{
                            element.parentNode.removeChild(element);
                        }}
                    }}
                ";

                await ExecuteScriptAsync(script);
            }
        }

        public async Task RemoveElementByOnClickAsync(string onClickValue)
        {
            var script = $@"
                var elements = document.querySelectorAll('.MainBt');
                for (var i = 0; i < elements.length; i++) {{
                    var element = elements[i];
                    if (element.getAttribute('onclick') === '{onClickValue}') {{
                        element.parentNode.removeChild(element);
                        break;
                    }}
                }}
            ";
            await ExecuteScriptAsync(script);
        }

        public async Task RemoveElementByHrefAsync(string href)
        {
            var script = $@"
                var elements = document.querySelectorAll('.top-nav__item');
                for (var i = 0; i < elements.length; i++) {{
                    var element = elements[i];
                    var anchor = element.querySelector('a');
                    if (anchor && anchor.getAttribute('href') === '{href}') {{
                        element.parentNode.removeChild(element);
                        break;
                    }}
                }}
            ";
            await ExecuteScriptAsync(script);
        }

        public async Task InsertTextAsync(string text)
        {
            // Escape single quotes in text
            text = text.Replace("'", "\\'");
            
            var script = $@"
                var focusedElement = document.activeElement;
                if (focusedElement && focusedElement.tagName === 'INPUT') {{
                    focusedElement.value += '{text}';
                    focusedElement.focus();
                }}
            ";
            await ExecuteScriptAsync(script);
        }

        public async Task DeleteLastCharacterAsync()
        {
            var script = @"
                var focusedElement = document.activeElement;
                if (focusedElement && focusedElement.tagName === 'INPUT' && focusedElement.value.length > 0) {
                    focusedElement.value = focusedElement.value.slice(0, -1);
                    focusedElement.focus();
                }
            ";
            await ExecuteScriptAsync(script);
        }

        public async Task PressEnterAsync()
        {
            var script = @"
                var focusedElement = document.activeElement;
                if (focusedElement) {
                    var event = new Event('keydown', { bubbles: true, cancelable: true, key: 'Enter', keyCode: 13 });
                    focusedElement.dispatchEvent(event);
                    focusedElement.focus();
                }
            ";
            await ExecuteScriptAsync(script);
        }
    }
}
