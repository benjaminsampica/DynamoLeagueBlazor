using Microsoft.Extensions.DependencyInjection;
using MockHttp.Json;
using MockHttp.Json.Newtonsoft;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MudBlazor;
using MudBlazor.Services;

namespace DynamoLeagueBlazor.Tests;

public class UITestBase : TestContextWrapper, IDisposable
{
    protected Mock<ISnackbar> MockSnackbar = null!;
    private readonly MockHttpHandler _mockHttpHandler = null!;

    public UITestBase()
    {
        var testContext = new TestContext();
        var mockSnackBar = new Mock<ISnackbar>();

        testContext.Services.AddMudServices();
        testContext.Services.AddSingleton(mockSnackBar.Object);
        _mockHttpHandler = testContext.Services.AddMockHttpClient();
        testContext.JSInterop.SetupVoid();

        TestContext = testContext;
        MockSnackbar = mockSnackBar;
    }

    public void Dispose() => TestContext?.Dispose();

    public MockHttpHandler GetHttpHandler => _mockHttpHandler;
}

public static class UITestExtensions
{
    public static MockHttpHandler AddMockHttpClient(this TestServiceProvider services)
    {
        var mockHttpHandler = new MockHttpHandler();
        var httpClient = new HttpClient(mockHttpHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        services.AddSingleton(httpClient);

        return mockHttpHandler;
    }

    public static IConfiguredRequest When(this MockHttpHandler mockHttpHandler, HttpMethod httpMethod, string? uri = null)
        => mockHttpHandler.When(matching =>
        {
            matching.Method(httpMethod);

            if (uri is not null) matching.RequestUri(uri);
        });

    public static ISequenceResponseResult RespondsWithJson<T>(this IConfiguredRequest request, T value)
        => request.RespondJson(HttpStatusCode.OK, value);
}