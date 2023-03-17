namespace DynamoLeagueBlazor.Server.Models;

public record Bid : BaseEntity
{
    public const int MinimumAmount = 1;

    public required int Amount { get; set; }
    public required int PlayerId { get; init; }
    public required int TeamId { get; init; }
    public bool IsOverBid { get; set; }
    public DateTimeOffset CreatedOn { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;
    public Team Team { get; private set; } = null!;
    public Player Player { get; private set; } = null!;
}

public static class BidExtensions
{
    public static Bid? FindHighestBid(this IEnumerable<Bid> bids)
        => bids.OrderByDescending(b => b.Amount).FirstOrDefault();
}
