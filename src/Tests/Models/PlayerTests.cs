using DynamoLeagueBlazor.Server.Models;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Tests.Models;

public class PlayerTests
{
    [Fact]
    public void GivenAPlayerWithNoBids_WhenFindingTheHighestBid_ThenTheHighestBidIsTheMinimumAmount()
        => CreateFakePlayer()
            .GetHighestBidAmount()
            .Should().Be(Bid.MinimumAmount);

    [Fact]
    public void GivenAPlayerWithABid_ThenReturnsThatBidAmount()
    {
        var player = CreateFakePlayer();

        player.AddBid(int.MaxValue, int.MaxValue);

        player.GetHighestBidAmount().Should().Be(int.MaxValue);
    }
}

public class PlayerStateTests
{
    [Fact]
    public void GivenAFreeAgent_ThenCanMoveToOfferMatching()
    {
        var player = CreateFakePlayer();
        player.State = PlayerState.FreeAgent;

        player.SetToOfferMatching();
    }

    [Fact]
    public void GivenABrandNewPlayer_ThenCanGoThroughTheCompleteLifetime()
    {
        var player = CreateFakePlayer();
        player.State = PlayerState.FreeAgent;

        FluentActions.Invoking(() => player.SetToOfferMatching()).Should().NotThrow();
    }
}
