using DynamoLeagueBlazor.Server.Features.FreeAgents;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class EndBiddingTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAPlayerWhoIsntAFreeAgent_ThenNothingHappens()
    {
        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.Unrostered;
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<EndBiddingService>();

        await sut.Invoke();

        var player = await FirstOrDefaultAsync<Player>();
        player!.State.Should().Be(mockPlayer.State);
    }

    [Fact]
    public async Task GivenAPlayerWhoIsAFreeAgentAndEndFreeAgencyIsNowOrBeforeNow_ThenChangesToOfferMatching()
    {
        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.FreeAgent;
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddDays(-1);
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<EndBiddingService>();

        await sut.Invoke();

        var player = await FirstOrDefaultAsync<Player>();
        player!.State.Should().Be(PlayerState.OfferMatching);
    }
}
