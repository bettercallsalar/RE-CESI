using System.Text.Json;

namespace RESR.MAUI.Controls;

public partial class RichTextEditor : ContentView
{
    private const string HtmlTemplate = """
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <style>
                :root {
                    color-scheme: light dark;
                }
                body {
                    margin: 0;
                    padding: 10px;
                    font-family: -apple-system, "Segoe UI", Roboto, Arial, sans-serif;
                    font-size: 14px;
                    background: transparent;
                }
                #editor {
                    min-height: 120px;
                    outline: none;
                    white-space: pre-wrap;
                }
                #editor:empty:before {
                    content: attr(data-placeholder);
                    color: #9ca3af;
                }
            </style>
        </head>
        <body>
            <div id="editor" contenteditable="true" data-placeholder="{0}"></div>
            <script>
                function exec(command) {{
                    document.execCommand(command, false, null);
                }}
                function execWithValue(command, value) {{
                    document.execCommand(command, false, value);
                }}
                function getHtml() {{
                    return document.getElementById('editor').innerHTML;
                }}
                function getText() {{
                    return document.getElementById('editor').innerText || '';
                }}
                function setHtml(value) {{
                    document.getElementById('editor').innerHTML = value || '';
                }}
            </script>
        </body>
        </html>
        """;

    private readonly TaskCompletionSource<bool> _readyTcs = new();

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(RichTextEditor), string.Empty, propertyChanged: OnPlaceholderChanged);

    public RichTextEditor()
    {
        InitializeComponent();
        LoadHtml();
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public async Task<string> GetHtmlAsync()
    {
        var result = await TryEvaluateJavaScriptAsync("getHtml()");
        return DeserializeJsString(result) ?? string.Empty;
    }

    public async Task<int> GetTextLengthAsync()
    {
        var result = await TryEvaluateJavaScriptAsync("getText().length");
        return int.TryParse(result, out var value) ? value : 0;
    }

    public async Task SetHtmlAsync(string html)
    {
        var safeValue = JsonSerializer.Serialize(html ?? string.Empty);
        await TryEvaluateJavaScriptAsync($"setHtml({safeValue})");
    }

    private void LoadHtml()
    {
        var placeholder = Placeholder?.Replace("'", "&#39;") ?? string.Empty;
        EditorWebView.Source = new HtmlWebViewSource
        {
            Html = string.Format(HtmlTemplate, placeholder)
        };
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (_readyTcs.Task.IsCompleted)
            return;

        _readyTcs.TrySetResult(true);
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var editor = (RichTextEditor)bindable;
        editor.LoadHtml();
    }

    private async void OnBoldClicked(object? sender, EventArgs e) => await ExecuteCommandSafeAsync("bold");
    private async void OnItalicClicked(object? sender, EventArgs e) => await ExecuteCommandSafeAsync("italic");
    private async void OnUnderlineClicked(object? sender, EventArgs e) => await ExecuteCommandSafeAsync("underline");
    private async void OnBulletedClicked(object? sender, EventArgs e) => await ExecuteCommandSafeAsync("insertUnorderedList");
    private async void OnNumberedClicked(object? sender, EventArgs e) => await ExecuteCommandSafeAsync("insertOrderedList");

    private async void OnLinkClicked(object? sender, EventArgs e)
    {
        try
        {
            var page = Window?.Page ?? Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
                return;

            var url = await page.DisplayPromptAsync("Ajouter un lien", "URL du lien");
            if (string.IsNullOrWhiteSpace(url))
                return;

            await ExecuteCommandSafeAsync("createLink", url);
        }
 #if DEBUG
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RichTextEditor link command failed: {ex}");
        }
 #else
        catch (Exception)
        {
        }
 #endif
    }

    private async Task ExecuteCommandSafeAsync(string command)
    {
        await TryEvaluateJavaScriptAsync($"exec('{command}')");
    }

    private async Task ExecuteCommandSafeAsync(string command, string value)
    {
        var safeValue = JsonSerializer.Serialize(value);
        await TryEvaluateJavaScriptAsync($"execWithValue('{command}', {safeValue})");
    }

    private async Task<string?> TryEvaluateJavaScriptAsync(string script)
    {
        try
        {
            await _readyTcs.Task;
            return await EditorWebView.EvaluateJavaScriptAsync(script);
        }
 #if DEBUG
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RichTextEditor JS execution failed: {ex}");
            return null;
        }
 #else
        catch (Exception)
        {
            return null;
        }
 #endif
    }

    private static string? DeserializeJsString(string? jsResult)
    {
        if (string.IsNullOrWhiteSpace(jsResult))
            return string.Empty;

        try
        {
            return JsonSerializer.Deserialize<string>(jsResult);
        }
        catch (JsonException)
        {
            return jsResult;
        }
    }
}
