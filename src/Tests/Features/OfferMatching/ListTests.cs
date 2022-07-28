using DynamoLeagueBlazor.Client.Features.OfferMatching;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using static DynamoLeagueBlazor.Shared.Features.OfferMatching.OfferMatchingListResult;

namespace DynamoLeagueBlazor.Tests.Features.OfferMatching;

public class ListServerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(OfferMatchingListRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsNoPlayersWhoAreOfferMatching_ThenReturnsNothing()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<OfferMatchingListResult>(OfferMatchingListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.OfferMatches.Should().HaveCount(0);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWhoIsInOfferMatching_ThenReturnsOneOfferMatching()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        mockPlayer.State = PlayerState.OfferMatching;
        await application.AddAsync(mockPlayer);

        var bidAmount = int.MaxValue;
        mockPlayer.AddBid(bidAmount, mockTeam.Id);
        await application.UpdateAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<OfferMatchingListResult>(OfferMatchingListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.OfferMatches.Should().HaveCount(1);

        var freeAgent = result.OfferMatches.First();
        freeAgent.Id.Should().Be(mockPlayer.Id);
        freeAgent.Name.Should().Be(mockPlayer.Name);
        freeAgent.Position.Should().Be(mockPlayer.Position);
        freeAgent.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        freeAgent.OfferingTeam.Should().Be(mockTeam.Name);
        freeAgent.Offer.Should().Be(bidAmount);
        freeAgent.RemainingTime.Should().BeCloseTo(mockPlayer.GetRemainingFreeAgencyTime(), TimeSpan.FromSeconds(1));
        freeAgent.CurrentUserIsOfferMatching.Should().BeTrue();
    }
}

public class ListClientTests : UITestBase
{
    [Fact]
    public void WhenThePageIsFirstLoaded_ThenShowsAListOfOfferMatches()
    {
        GetHttpHandler.When(HttpMethod.Get, OfferMatchingListRouteFactory.Uri)
            .RespondsWithJson(AutoFaker.Generate<OfferMatchingListResult>());

        var cut = RenderComponent<List>();

        cut.Markup.Contains("<tr>");

        GetHttpHandler.Verify();
    }

    [Fact]
    public void GivenAnOfferMatch_WhenTheCurrentUserIsntTheOfferingTeam_ThenDoesNotShowTheOfferMatchButton()
    {
        var mockItem = new AutoFaker<OfferMatchingItem>()
            .RuleFor(omi => omi.CurrentUserIsOfferMatching, false)
            .Generate();
        var mockResult = new AutoFaker<OfferMatchingListResult>()
            .RuleFor(omlr => omlr.OfferMatches, faker => new List<OfferMatchingItem> { mockItem })
            .Generate();
        GetHttpHandler.When(HttpMethod.Get, OfferMatchingListRouteFactory.Uri)
            .RespondsWithJson(mockResult);

        var cut = RenderComponent<List>();

        cut.Markup.Should().NotContain("matchButton");
    }

    [Fact]
    public void GivenAnOfferMatch_WhenTheCurrentUserIsTheOfferingTeam_ThenDoesShowTheOfferMatchButton()
    {
        var mockItem = new AutoFaker<OfferMatchingItem>()
            .RuleFor(omi => omi.CurrentUserIsOfferMatching, true)
            .Generate();
        var mockResult = new AutoFaker<OfferMatchingListResult>()
            .RuleFor(omlr => omlr.OfferMatches, faker => new List<OfferMatchingItem> { mockItem })
            .Generate();
        GetHttpHandler.When(HttpMethod.Get, OfferMatchingListRouteFactory.Uri)
            .RespondsWithJson(mockResult);

        var cut = RenderComponent<List>();

        cut.Markup.Should().Contain("matchButton");
    }
}
