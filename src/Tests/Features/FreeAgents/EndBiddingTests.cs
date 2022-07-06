using DynamoLeagueBlazor.Server.Features.FreeAgents;
using DynamoLeagueBlazor.Server.Models;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class EndBiddingTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAPlayerWhoIsntAFreeAgent_ThenNothingHappens()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.Unrostered;
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<EndBiddingService>();

        await sut.Invoke();

        var player = await application.FirstOrDefaultAsync<Player>();
        player!.State.Should().Be(mockPlayer.State);
    }

    [Fact]
    public async Task GivenAPlayerWhoIsAFreeAgentAndEndFreeAgencyIsNowOrBeforeNow_ThenChangesToOfferMatching()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.FreeAgent;
        mockPlayer.EndOfFreeAgency = DateTime.Now;
        await application.AddAsync(mockPlayer);

        var sut = GetRequiredService<EndBiddingService>();

        await sut.Invoke();

        var player = await application.FirstOrDefaultAsync<Player>();
        player!.State.Should().Be(PlayerState.OfferMatching);
    }
}
