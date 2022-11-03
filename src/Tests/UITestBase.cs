using Bunit.TestDoubles;
using DynamoLeagueBlazor.Client.Shared.Components;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using MockHttp.Json;
using MockHttp.Json.Newtonsoft;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MudBlazor.Services;
using System.Security.Claims;

namespace DynamoLeagueBlazor.Tests;

public class UITestBase : TestContextWrapper, IDisposable
{
    private readonly MockHttpHandler _mockHttpHandler = null!;

    private readonly TestAuthorizationContext _testAuthorizationContext = null!;
    protected Mock<ISnackbar> MockSnackbar = null!;

    public UITestBase()
    {
        var testContext = new TestContext();
        var mockSnackBar = new Mock<ISnackbar>();

        testContext.Services.AddMudServices();
        testContext.Services.AddSingleton(mockSnackBar.Object);
        testContext.Services.AddSingleton(Mock.Of<IConfirmDialogService>());
        testContext.JSInterop.SetupVoid();

        var mockHttpHandler = new MockHttpHandler();
        var httpClient = new HttpClient(mockHttpHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        testContext.Services.AddSingleton(httpClient);
        _mockHttpHandler = mockHttpHandler;

        _testAuthorizationContext = testContext.AddTestAuthorization();
        TestContext = testContext;
        MockSnackbar = mockSnackBar;
    }

    public void Dispose() => TestContext?.Dispose();

    public MockHttpHandler GetHttpHandler => _mockHttpHandler;

    public void AuthorizeAsUser(int teamId, bool adminApproved = true)
    {
        var authorizedState = _testAuthorizationContext.SetAuthorized(RandomString);
        authorizedState.SetClaims(
            new Claim(nameof(IUser.Approved), adminApproved.ToString()),
            new Claim(nameof(IUser.TeamId), teamId.ToString()));

        if (adminApproved)
        {
            authorizedState.SetPolicies(PolicyRequirements.IsAdminApproved);
        }
    }
}

public static class UITestExtensions
{
    public static IConfiguredRequest When(this MockHttpHandler mockHttpHandler, HttpMethod httpMethod, string? uri = null)
        => mockHttpHandler.When(matching =>
        {
            matching.Method(httpMethod);

            if (uri is not null) matching.RequestUri(uri);
        });

    public static ISequenceResponseResult RespondsWithJson<T>(this IConfiguredRequest request, T value)
        => request.RespondJson(HttpStatusCode.OK, value);
}