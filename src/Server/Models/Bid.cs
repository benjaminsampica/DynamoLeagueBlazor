namespace DynamoLeagueBlazor.Server.Models;

public record Bid : BaseEntity
{
    public const int MinimumAmount = 1;

    public Bid(int amount, int teamId, int playerId, bool isOverBid)
    {
        PlayerId = playerId;
        Amount = amount;
        TeamId = teamId;
        IsOverBid = isOverBid;
    }

    public int Amount { get; set; }
    public int PlayerId { get; private set; }
    public int TeamId { get; private set; }
    public bool IsOverBid { get; set; }
    public DateTime CreatedOn { get; private set; } = DateTime.Now;
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.Now;
    public Team Team { get; private set; } = null!;
    public Player Player { get; private set; } = null!;
}

public static class BidExtensions
{
    public static Bid? FindHighestBid(this IEnumerable<Bid> bids)
        => bids.OrderByDescending(b => b.Amount).FirstOrDefault();
}
