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
            new(1, int.MaxValue, int.MaxValue),
            new(2, int.MaxValue, int.MaxValue)
        };

        bids.FindHighestBid().Should().NotBeNull();
        bids.FindHighestBid()!.Amount.Should().Be(2);
    }
}
