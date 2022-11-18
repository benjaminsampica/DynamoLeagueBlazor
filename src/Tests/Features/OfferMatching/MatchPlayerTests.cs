using DynamoLeagueBlazor.Shared.Features.OfferMatching;

namespace DynamoLeagueBlazor.Tests.Features.OfferMatching;

public class MatchPlayerServerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerIsMatchedAndHasBids_ThenPlayerIsMovedToUnsignedForTheMatchingTeam()
    {
        var matchingTeam = CreateFakeTeam();
        await AddAsync(matchingTeam);

        var biddingTeam = CreateFakeTeam();
        await AddAsync(biddingTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = matchingTeam.Id;
        mockPlayer.State = PlayerState.OfferMatching;
        mockPlayer.AddBid(Bid.MinimumAmount, biddingTeam.Id);
        mockPlayer.EndOfFreeAgency = DateTime.Today.AddDays(-3);
        await AddAsync(mockPlayer);

        var request = new MatchPlayerRequest(mockPlayer.Id);
        var application = GetUserAuthenticatedApplication(matchingTeam.Id);
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(MatchPlayerRouteFactory.Uri, request);

        response.Should().BeSuccessful();

        var unsignedPlayer = await FirstOrDefaultAsync<Player>();
        unsignedPlayer!.YearContractExpires.Should().Be(null);
        unsignedPlayer.EndOfFreeAgency.Should().Be(null);
        unsignedPlayer.YearAcquired.Should().Be(DateTime.Today.Year);
        unsignedPlayer.ContractValue.Should().Be(Bid.MinimumAmount);
        unsignedPlayer.State.Should().Be(PlayerState.Unsigned);
        unsignedPlayer.TeamId.Should().Be(matchingTeam.Id);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerHasNoBidsOnOfferMatch_ThenContractValueIsTheMinimumBid()
    {
        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        mockPlayer.State = PlayerState.OfferMatching;
        await AddAsync(mockPlayer);

        var request = new MatchPlayerRequest(mockPlayer.Id);
        var application = GetUserAuthenticatedApplication(mockTeam.Id);
        var client = application.CreateClient();

        await client.PostAsJsonAsync(MatchPlayerRouteFactory.Uri, request);

        var result = await FirstOrDefaultAsync<Player>();
        result!.ContractValue.Should().Be(Bid.MinimumAmount);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenTheyTryToOfferMatchOnAPlayerThatIsntTheirs_ThenReturnsAClientError()
    {
        var application = GetUserAuthenticatedApplication();
        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.OfferMatching;
        await AddAsync(mockPlayer);

        var request = new MatchPlayerRequest(mockPlayer.Id);
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(MatchPlayerRouteFactory.Uri, request);
        response.Should().HaveClientError();

        var result = await FirstOrDefaultAsync<Player>();
        result!.TeamId.Should().NotBe(mockTeam.Id);
    }
}

public class MatchPlayerRequestValidatorTests
{
    [Theory]
    [InlineData(-1), InlineData(0)]
    public void GivenInvalidPlayerIds_ThenAreNotValid(int playerId) =>
        new MatchPlayerRequestValidator(Mock.Of<IMatchPlayerValidator>()).TestValidate(new MatchPlayerRequest(playerId)).ShouldHaveValidationErrorFor(p => p.PlayerId);
}