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

    [Fact]
    public void GivenAPlayerWithNoBids_WhenFindingTheHighestBiddingTeam_ThenEmptyString()
       => CreateFakePlayer()
           .GetOfferingTeam()
           .Should().Be(string.Empty);

    [Fact]
    public async Task GivenAPlayerWithABid_ThenReturnsOfferingTeamAsync()
    {

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var player = CreateFakePlayer();
        player.AddBid(int.MaxValue, stubTeam.Id);

        player.GetOfferingTeam().Should().Be(player.Bids.ElementAt(0).Team.Name);
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
