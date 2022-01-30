using Microsoft.Extensions.DependencyInjection;
using MockHttp;
using MockHttp.Json;
using MockHttp.Json.Newtonsoft;
using MockHttp.Language;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace DynamoLeagueBlazor.Tests;

[TestFixture]
internal class UITestBase : TestContextWrapper
{
    protected Mock<ISnackbar> MockSnackbar = null!;
    private MockHttpHandler _mockHttpHandler = null!;

    [SetUp]
    public void Setup()
    {
        var testContext = new Bunit.TestContext();
        var mockSnackBar = new Mock<ISnackbar>();

        testContext.Services.AddMudServices();
        testContext.Services.AddSingleton(mockSnackBar.Object);
        _mockHttpHandler = testContext.Services.AddMockHttpClient();
        testContext.JSInterop.SetupVoid();

        TestContext = testContext;
        MockSnackbar = mockSnackBar;
    }

    [TearDown]
    public void TearDown() => TestContext?.Dispose();

    public MockHttpHandler GetHttpHandler => _mockHttpHandler;
}

internal static class UITestExtensions
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

    public static IConfiguredRequest When(this MockHttpHandler mockHttpHandler, HttpMethod httpMethod, string uri)
        => mockHttpHandler.When(matching =>
            matching
                .Method(httpMethod)
                .RequestUri(uri)
        );

    public static void RespondsWithJson<T>(this IConfiguredRequest request, T value)
        => request.RespondJson(HttpStatusCode.OK, value);
}