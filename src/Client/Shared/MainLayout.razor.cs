using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace DynamoLeagueBlazor.Client.Shared;

public partial class MainLayout
{
    [Inject] private IWebAssemblyHostEnvironment HostEnvironment { get; set; } = null!;
    private MudThemeProvider? _mudThemeProvider;
    private bool _isDarkMode;
    private static readonly string[] _headerFontFamily = new[] { "Graduate", "cursive" };
    private Color buttonColor;
    private Color appBarColor;
    private MudTheme _baseTheme = new MudTheme()
    {
        Typography = new Typography
        {
            Default = new Default { FontFamily = new[] { "Open Sans", "sans-serif" } },
            H1 = new H1 { FontFamily = _headerFontFamily },
            H2 = new H2 { FontFamily = _headerFontFamily },
            H3 = new H3 { FontFamily = _headerFontFamily },
            H4 = new H4 { FontFamily = _headerFontFamily },
            H5 = new H5 { FontFamily = _headerFontFamily },
            H6 = new H6 { FontFamily = _headerFontFamily }
        }
    };
    private ErrorBoundary? _errorBoundary;
    private bool _showNavMenu;

    protected override void OnInitialized()
    {
        buttonColor = _isDarkMode ? Color.Primary : Color.Dark;
        appBarColor = _isDarkMode ? Color.Dark : Color.Primary;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = await _mudThemeProvider!.GetSystemPreference();
            StateHasChanged();
        }
    }

    protected override void OnParametersSet()
    {
        _errorBoundary?.Recover();
    }

    private void OnOpenChanged(bool open = true) => _showNavMenu = open;

    private string GetErrorMessage(Exception ex)
    {
        var genericMessage = "An error has occured...";
        if (!HostEnvironment.IsProduction())
        {
            var customMessage = ex switch
            {
                JSException when ex.Message.Contains("Could not load settings") => "The authentication server is not responding. Please ensure the DynamoLeague.Server project is the startup project and try running again.",
                _ => ex.ToString()
            };

            return customMessage;
        }

        return genericMessage;
    }
}
