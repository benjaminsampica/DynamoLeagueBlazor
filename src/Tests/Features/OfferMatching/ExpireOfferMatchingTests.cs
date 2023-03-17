using DynamoLeagueBlazor.Server.Features.OfferMatching;

namespace DynamoLeagueBlazor.Tests.Features.OfferMatching;

public class ExpireOfferMatchingTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAnNonOfferMatchingPlayer_ThenDoesNothing()
    {
        var mockPlayer = CreateFakePlayer();
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var ignoredPlayer = await FirstOrDefaultAsync<Player>();
        ignoredPlayer.Should().BeEquivalentTo(mockPlayer);
    }

    [Theory]
    [InlineData(0), InlineData(-1), InlineData(-2)]
    public async Task GivenAnOfferMatchingPlayer_WhenTodayIsTwoDaysOrLessAfterEndOfFreeAgency_ThenDoesNothing(int daysAgo)
    {
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddDays(daysAgo);
        mockPlayer.State = PlayerState.OfferMatching;
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var offerMatchingPlayer = await FirstOrDefaultAsync<Player>();
        offerMatchingPlayer.Should().BeEquivalentTo(mockPlayer);
    }

    [Fact]
    public async Task GivenAnOfferMatchingPlayerWithBids_WhenTodayIsThreeDaysOrMoreAfterEndOfFreeAgency_ThenSetsToUnsignedForTheBiddingTeam()
    {
        var biddingTeam = CreateFakeTeam();
        await AddAsync(biddingTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.OfferMatching;
        mockPlayer.AddBid(Bid.MinimumAmount, biddingTeam.Id);
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddDays(-3);
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var unsignedPlayer = await FirstOrDefaultAsync<Player>();
        unsignedPlayer!.State.Should().Be(PlayerState.Unsigned);
        unsignedPlayer!.TeamId.Should().Be(biddingTeam.Id);
        unsignedPlayer.ContractValue.Should().Be(Bid.MinimumAmount);
    }

    [Fact]
    public async Task GivenAnOfferMatchingPlayerWithoutBids_WhenTodayIsThreeDaysOrMoreAfterEndOfFreeAgency_ThenIsRemovedFromTheLeague()
    {
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddDays(-3);
        mockPlayer.State = PlayerState.OfferMatching;
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var offerMatchingPlayer = await FirstOrDefaultAsync<Player>();
        offerMatchingPlayer.Should().BeNull();
    }
}
