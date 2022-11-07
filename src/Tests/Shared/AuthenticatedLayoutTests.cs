using DynamoLeagueBlazor.Client.Shared;

namespace DynamoLeagueBlazor.Tests.Shared;

public class AuthenticatedLayoutTests : UITestBase
{
    [Fact]
    public void GivenAUserIsNotAuthenticated_ThenRedirectsToLogin()
    {
        var cut = RenderComponent<AuthenticatedLayout>();

        cut.HasComponent<RedirectToLogin>().Should().BeTrue();
    }

    [Fact]
    public void GivenAUserIsNotYetApprovedByAnAdmin_ThenShowsAwaitingApprovalMessage()
    {
        AuthorizeAsUser(int.MaxValue, false);

        var cut = RenderComponent<AuthenticatedLayout>();

        cut.HasComponent<MudAlert>().Should().BeTrue();
    }

    [Fact]
    public void GivenAUserIsApprovedByAnAdmin_ThenShowsThePage()
    {
        AuthorizeAsUser(int.MaxValue, true);

        var cut = RenderComponent<AuthenticatedLayout>();

        cut.HasComponent<MudAlert>().Should().BeFalse();
    }
}
