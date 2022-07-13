using DynamoLeagueBlazor.Server.Features.OfferMatching;
using DynamoLeagueBlazor.Server.Models;

namespace DynamoLeagueBlazor.Tests.Features.OfferMatching;

public class ExpireOfferMatchingTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAnNonOfferMatchingPlayer_ThenDoesNothing()
    {
        var application = CreateUnauthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var ignoredPlayer = await application.FirstOrDefaultAsync<Player>();
        ignoredPlayer.Should().BeEquivalentTo(mockPlayer);
    }

    [Theory]
    [InlineData(0), InlineData(-1), InlineData(-2)]
    public async Task GivenAnOfferMatchingPlayer_WhenTodayIsTwoDaysOrLessAfterEndOfFreeAgency_ThenDoesNothing(int daysAgo)
    {
        var application = CreateUnauthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Today.AddDays(daysAgo);
        mockPlayer.State = PlayerState.OfferMatching;
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var offerMatchingPlayer = await application.FirstOrDefaultAsync<Player>();
        offerMatchingPlayer.Should().BeEquivalentTo(mockPlayer);
    }

    [Fact]
    public async Task GivenAnOfferMatchingPlayerWithBids_WhenTodayIsThreeDaysOrMoreAfterEndOfFreeAgency_ThenSetsToUnsignedForTheBiddingTeam()
    {
        var application = CreateUnauthenticatedApplication();

        var biddingTeam = CreateFakeTeam();
        await application.AddAsync(biddingTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.OfferMatching;
        mockPlayer.AddBid(int.MaxValue, biddingTeam.Id);
        mockPlayer.EndOfFreeAgency = DateTime.Today.AddDays(-3);
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var unsignedPlayer = await application.FirstOrDefaultAsync<Player>();
        unsignedPlayer!.State.Should().Be(PlayerState.Unsigned);
        unsignedPlayer!.TeamId.Should().Be(biddingTeam.Id);
        unsignedPlayer.ContractValue.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task GivenAnOfferMatchingPlayerWithoutBids_WhenTodayIsThreeDaysOrMoreAfterEndOfFreeAgency_ThenIsRemovedFromTheLeague()
    {
        var application = CreateUnauthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Today.AddDays(-3);
        mockPlayer.State = PlayerState.OfferMatching;
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var offerMatchingPlayer = await application.FirstOrDefaultAsync<Player>();
        offerMatchingPlayer.Should().BeNull();
    }
}
