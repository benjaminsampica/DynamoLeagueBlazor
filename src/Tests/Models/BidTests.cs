namespace DynamoLeagueBlazor.Tests.Models;

public class BidExtensionsTests
{
    [Fact]
    public void FindHighestBid_GivenNoBids_ThenReturnsNothing()
    {
        var bids = new List<Bid>();

        bids.FindHighestBid().Should().BeNull();
    }

    [Fact]
    public void FindHighestBid_GivenTwoBids_ThenReturnsTheHighestOne()
    {
        var bids = new List<Bid>()
        {
            new() { Amount = 1, TeamId = int.MaxValue, PlayerId = int.MaxValue },
            new() { Amount = 2, TeamId = int.MaxValue, PlayerId = int.MaxValue }
        };

        bids.FindHighestBid().Should().NotBeNull();
        bids.FindHighestBid()!.Amount.Should().Be(2);
    }
}
