using DynamoLeagueBlazor.Client.Features.OfferMatching;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using System.Net.Http.Json;

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
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerIsMatchedAndHasBids_ThenPlayerIsMovedToUnsignedForTheMatchingTeam()
    {
        var application = CreateUserAuthenticatedApplication();

        var biddingTeam = CreateFakeTeam();
        await application.AddAsync(biddingTeam);

        var matchingTeam = CreateFakeTeam();
        await application.AddAsync(matchingTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = matchingTeam.Id;
        mockPlayer.State = PlayerState.OfferMatching;
        mockPlayer.AddBid(int.MaxValue, biddingTeam.Id);
        mockPlayer.EndOfFreeAgency = DateTime.Today.AddDays(-3);
        await application.AddAsync(mockPlayer);

        var request = new MatchPlayerRequest
        {
            PlayerId = mockPlayer.Id
        };
        var client = application.CreateClient();

        await client.PostAsJsonAsync(OfferMatchingListRouteFactory.Uri, request);

        var unsignedPlayer = await application.FirstOrDefaultAsync<Player>();
        unsignedPlayer!.YearContractExpires.Should().Be(null);
        unsignedPlayer.EndOfFreeAgency.Should().Be(null);
        unsignedPlayer.YearAcquired.Should().Be(DateTime.Today.Year);
        unsignedPlayer.ContractValue.Should().Be(int.MaxValue);
        unsignedPlayer.State.Should().Be(PlayerState.Unsigned);
        unsignedPlayer.TeamId.Should().Be(matchingTeam.Id);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerHasNoBidsOnOfferMatch_ThenContractValueIsTheMinimumBid()
    {
        var application = CreateUserAuthenticatedApplication();
        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var player = CreateFakePlayer();
        player.TeamId = mockTeam.Id;
        player.State = PlayerState.OfferMatching;
        await application.AddAsync(player);

        var request = new MatchPlayerRequest
        {
            PlayerId = player.Id
        };
        var client = application.CreateClient();

        await client.PostAsJsonAsync(OfferMatchingListRouteFactory.Uri, request);

        var result = await application.FirstOrDefaultAsync<Player>();
        result!.ContractValue.Should().Be(Bid.MinimumAmount);
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
}
