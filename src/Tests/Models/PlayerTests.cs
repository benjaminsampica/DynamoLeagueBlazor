namespace DynamoLeagueBlazor.Tests.Models;

public class PlayerTests
{
    [Fact]
    public void GivenAPlayerWithNoBids_WhenFindingTheHighestBid_ThenTheHighestBidIsTheMinimumAmount()
        => CreateFakePlayer()
            .GetHighestBidAmount()
            .Should().Be(Bid.MinimumAmount);

    [Fact]
    public void GivenAPlayerWithABid_ThenReturnsTheHighestPublicBid()
    {
        var player = CreateFakePlayer();

        player.AddBid(int.MaxValue, int.MaxValue);

        player.GetHighestBidAmount().Should().Be(Bid.MinimumAmount);
    }

    [Fact]
    public void GivenAPlayerWithNoBids_WhenFindingTheHighestBiddingTeam_ThenEmptyString()
       => CreateFakePlayer()
           .GetOfferingTeam()
           .Should().Be(string.Empty);

    [Fact]
    public void GivenAPlayerHasNoBids_WhenATeamBidsOnThem_ThenThatTeamIsTheHighestBidder()
    {
        var player = CreateFakePlayer();

        player.AddBid(Bid.MinimumAmount, 1);

        var bid = player.Bids.FindHighestBid();
        bid.Should().NotBeNull();
        bid!.TeamId.Should().Be(1);
    }

    [Fact]
    public void GivenAPlayerHasNoBids_WhenATeamBidsOverTheMinimumBid_ThenThatTeamHasAPublicBidOfOneAndAPrivateBidThatIsHigher()
    {
        var player = CreateFakePlayer();

        player.AddBid(int.MaxValue, 1);

        player.Bids.Should().HaveCount(2);

        var publicBid = player.Bids.Where(b => b.IsOverBid == false).FirstOrDefault();
        publicBid.Should().NotBeNull();
        publicBid!.Amount.Should().Be(Bid.MinimumAmount);
        publicBid.IsOverBid.Should().BeFalse();

        var privateBid = player.Bids.FindHighestBid();
        privateBid.Should().NotBeNull();
        privateBid!.Amount.Should().Be(int.MaxValue);
        privateBid.IsOverBid.Should().BeTrue();
    }

    [Fact]
    public void GivenAPlayerHasABidFromTeamOne_WhenThatSameTeamBidsOnThemAgain_ThenIsAnOverbid()
    {
        var player = CreateFakePlayer();

        player.AddBid(1, 1);
        player.AddBid(2, 1);

        var bid = player.Bids.FindHighestBid();
        bid!.Amount.Should().Be(2);
        bid.IsOverBid.Should().BeTrue();
        bid.UpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GivenAPlayerHasABidFromTeamOne_WhenThatSameTeamTriesToBidLower_ThenIsIgnored()
    {
        var player = CreateFakePlayer();

        player.AddBid(1, 1);
        player.AddBid(4, 1);
        player.AddBid(3, 1);

        var bid = player.Bids.FindHighestBid();
        bid!.Amount.Should().Be(4);
    }

    [Fact]
    public void GivenAPlayerHasABidOfOneFromOneTeam_WhenAnotherTeamBidsTwo_ThenTheHighestBidIsFromTeamTwo()
    {
        var player = CreateFakePlayer();

        player.AddBid(1, 1);
        player.AddBid(2, 2);

        var bid = player.Bids.FindHighestBid();
        bid!.Amount.Should().Be(2);
        bid.IsOverBid.Should().BeFalse();
        bid.TeamId.Should().Be(2);
    }

    [Fact]
    public void GivenAPlayerHasABidOfOneAndOverbidOfFourFromOneTeam_WhenAnotherTeamBidsTwo_ThenCounterBidsFromTheFirstTeamToTwo()
    {
        var player = CreateFakePlayer();

        player.AddBid(1, 1);
        player.AddBid(4, 1);
        player.AddBid(2, 2);

        player.Bids.Should().HaveCount(4);

        var highestBid = player.Bids.FindHighestBid();
        highestBid!.Amount.Should().Be(4);
        highestBid.IsOverBid.Should().BeTrue();
        highestBid.TeamId.Should().Be(1);

        var counterBid = player.Bids.OrderByDescending(b => b.Amount).Skip(1).First();
        counterBid!.Amount.Should().Be(2);
        counterBid.IsOverBid.Should().BeFalse();
        counterBid.TeamId.Should().Be(1);
    }

    [Fact]
    public void GivenAPlayerHasABidOfOneAndOverbidOfFourFromOneTeam_WhenAnotherTeamBidsFour_ThenCounterBidsFromTheFirstTeamToTheHighestAmount()
    {
        var player = CreateFakePlayer();

        player.AddBid(1, 1);
        player.AddBid(4, 1);
        player.AddBid(4, 2);

        player.Bids.Should().HaveCount(3);

        var highestBid = player.Bids.FindHighestBid();
        highestBid!.Amount.Should().Be(4);
        highestBid.IsOverBid.Should().BeFalse();
        highestBid.TeamId.Should().Be(1);
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
        offerMatchingPlayer.TeamId = int.MaxValue;
        offerMatchingPlayer.State = PlayerState.OfferMatching;

        offerMatchingPlayer.MatchOffer();

        offerMatchingPlayer.State.Should().Be(PlayerState.Unsigned);
    }

    [Fact]
    public void GivenAnOfferMatchingPlayer_WhenMatchIsExpiring_ThenMovesToUnsigned()
    {
        var offerMatchingPlayer = CreateFakePlayer();
        offerMatchingPlayer.AddBid(int.MaxValue, int.MaxValue);
        offerMatchingPlayer.State = PlayerState.OfferMatching;

        offerMatchingPlayer.ExpireMatch();

        offerMatchingPlayer.State.Should().Be(PlayerState.Unsigned);
    }

    [Fact]
    public void GivenAnUnsignedPlayer_WhenATeamSignsThePlayer_ThenMovesToRostered()
    {
        var unsignedPlayer = CreateFakePlayer();
        unsignedPlayer.State = PlayerState.Unsigned;

        unsignedPlayer.SignForCurrentTeam(int.MaxValue, int.MaxValue);

        unsignedPlayer.State.Should().Be(PlayerState.Rostered);
    }

    [Fact]
    public void GivenAnRosteredPlayer_WhenANewSeasonStarts_ThenMovesToFreeAgent()
    {
        var rosteredPlayer = CreateFakePlayer();
        rosteredPlayer.State = PlayerState.Rostered;

        rosteredPlayer.BeginNewSeason(DateTime.MaxValue);

        rosteredPlayer.State.Should().Be(PlayerState.FreeAgent);
    }

    [Fact]
    public void GivenABrandNewPlayer_ThenCanGoThroughTheCompleteLifecycle()
    {
        var player = CreateFakePlayer();
        player.TeamId = int.MaxValue;

        FluentActions.Invoking(() =>
        {
            player.SignForCurrentTeam(int.MaxValue, int.MaxValue);
            player.BeginNewSeason(DateTime.MaxValue);
            player.EndBidding();
            player.MatchOffer();
        }).Should().NotThrow();

        player.State.Should().Be(PlayerState.Unsigned);
    }
}
