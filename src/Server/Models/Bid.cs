namespace DynamoLeagueBlazor.Server.Models;

public record Bid : BaseEntity
{
    public const int MinimumAmount = 1;

    public required int Amount { get; init; }
    public required int PlayerId { get; init; }
    public required int TeamId { get; init; }
    public DateTime CreatedOn { get; private set; } = DateTime.Now;
    public Team Team { get; private set; } = null!;
    public Player Player { get; private set; } = null!;
}

public static class BidExtensions
{
    public static Bid? FindHighestBid(this ICollection<Bid> bids)
        => bids.OrderByDescending(b => b.Amount).FirstOrDefault();
}
