using DynamoLeagueBlazor.Server.Models;

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
    public void GivenAFreeAgent_WhenEndingBidding_ThenMovesToOfferMatching()
    {
        var freeAgent = CreateFakePlayer();
        freeAgent.State = PlayerState.FreeAgent;

        freeAgent.EndBidding();

        freeAgent.State.Should().Be(PlayerState.OfferMatching);
    }

    [Fact]
    public void GivenAnOfferMatchingPlayer_WhenMatchingOffer_ThenMovesToUnsigned()
    {
        var offerMatchingPlayer = CreateFakePlayer();
        offerMatchingPlayer.State = PlayerState.OfferMatching;

        offerMatchingPlayer.MatchOffer();

        offerMatchingPlayer.State.Should().Be(PlayerState.Unsigned);
    }

    [Fact]
    public void GivenAnOfferMatchingPlayer_WhenMatchIsExpiring_ThenMovesToUnsigned()
    {
        var offerMatchingPlayer = CreateFakePlayer();
        offerMatchingPlayer.State = PlayerState.OfferMatching;

        offerMatchingPlayer.ExpireMatch();

        offerMatchingPlayer.State.Should().Be(PlayerState.Unsigned);
    }

    //[Fact]
    //public void GivenABrandNewPlayer_ThenCanGoThroughTheCompleteLifetime()
    //{
    //    var player = CreateFakePlayer();

    //    FluentActions.Invoking(() => player.SOMETHING()).Should().NotThrow();
    //}
}
