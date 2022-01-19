using JustEat.HttpClientInterception;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace DynamoLeagueBlazor.Tests;

internal class UITestBase : TestContextWrapper
{
    private Mock<ISnackbar> _mockSnackbar = null!;

    [SetUp]
    public void Setup()
    {
        var testContext = new Bunit.TestContext();
        var mockSnackBar = new Mock<ISnackbar>();

        testContext.Services.AddMudServices();
        testContext.Services.AddSingleton(mockSnackBar.Object);
        testContext.JSInterop.SetupVoid();

        TestContext = testContext;
        _mockSnackbar = mockSnackBar;
    }

    [TearDown]
    public void TearDown() => TestContext?.Dispose();

    private void BuildUnauthorizedRequest()
    {
        var options = new HttpClientInterceptorOptions();
        var builder = new HttpRequestInterceptionBuilder();

        builder
            .Requests()
            .ForGet()
            .ForHttps()
            .ForHost("public.je-apis.com")
            .RegisterWith(options);
    }
}
