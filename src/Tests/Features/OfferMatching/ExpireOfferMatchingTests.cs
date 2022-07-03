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
    public async Task GivenAnOfferMatchingPlayer_WhenTodayIsThreeDaysOrMoreAfterEndOfFreeAgency_ThenSetsToUnsigned()
    {
        var application = CreateUnauthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Today.AddDays(-3);
        mockPlayer.State = PlayerState.OfferMatching;
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<ExpireOfferMatchingService>();

        await sut.Invoke();

        var offerMatchingPlayer = await application.FirstOrDefaultAsync<Player>();
        offerMatchingPlayer!.State.Should().Be(PlayerState.Unsigned);
    }
}
