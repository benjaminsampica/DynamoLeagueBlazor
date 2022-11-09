using DynamoLeagueBlazor.Server.Features.FreeAgents;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class EndBiddingTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAPlayerWhoIsntAFreeAgent_ThenNothingHappens()
    {
        var application = GetUserAuthenticatedApplication();

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
        GetUserAuthenticatedApplication();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.FreeAgent;
        mockPlayer.EndOfFreeAgency = DateTime.Now;
        await AddAsync(mockPlayer);

        var sut = GetRequiredService<EndBiddingService>();

        await sut.Invoke();
        await Task.Delay(500); // Give time for the invocation

        var player = await FirstOrDefaultAsync<Player>();
        player!.State.Should().Be(PlayerState.OfferMatching);
    }
}
