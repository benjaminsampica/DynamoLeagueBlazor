using Bunit.TestDoubles;
using DynamoLeagueBlazor.Client.Shared.Components;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Components;
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
    private readonly MockHttpHandler _mockHttpHandler = new();
    private readonly TestAuthorizationContext _testAuthorizationContext = null!;

    protected Mock<ISnackbar> MockSnackbar = new();

    public UITestBase()
    {
        var testContext = new TestContext();
        testContext.Services.AddMudServices();
        testContext.Services.AddSingleton(Mock.Of<IConfirmDialogService>());
        testContext.JSInterop.SetupVoid();
        TestContext = testContext;

        _testAuthorizationContext = testContext.AddTestAuthorization();

        testContext.Services.AddSingleton(MockSnackbar.Object);

        var httpClient = new HttpClient(_mockHttpHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        testContext.Services.AddSingleton(httpClient);
    }

    public MockHttpHandler GetHttpHandler => _mockHttpHandler;

    public async Task<IRenderedComponent<MudDialogProvider>> RenderMudDialogAsync<TComponent>(DialogParameters? dialogParameters = null)
        where TComponent : ComponentBase
    {
        var mudDialogProvider = RenderComponent<MudDialogProvider>();
        var dialogService = TestContext!.Services.GetRequiredService<IDialogService>();

        if (dialogParameters is null)
        {
            await mudDialogProvider.InvokeAsync(() => dialogService.Show<TComponent>());
        }
        else
        {
            await mudDialogProvider.InvokeAsync(() => dialogService.Show<TComponent>(string.Empty, dialogParameters));
        }

        return mudDialogProvider;
    }

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

    public void AuthorizeAsAdmin(int teamId)
    {
        var authorizedState = _testAuthorizationContext.SetAuthorized(RandomString);
        authorizedState.SetClaims(
            new Claim(nameof(IUser.Approved), bool.TrueString),
            new Claim(nameof(IUser.TeamId), teamId.ToString()));

        _testAuthorizationContext.SetPolicies(PolicyRequirements.Admin, PolicyRequirements.IsAdminApproved);
    }

    public void Dispose() => TestContext?.Dispose();
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