namespace DynamoLeagueBlazor.Server.Models;

public record Bid : BaseEntity
{
    public const int MinimumAmount = 1;

    public Bid(int amount, int teamId, int playerId)
    {
        PlayerId = playerId;
        Amount = amount;
        TeamId = teamId;
    }

    public int Amount { get; private set; }
    public int PlayerId { get; private set; }
    public int TeamId { get; private set; }
    public DateTime CreatedOn { get; private set; } = DateTime.Now;
    public Team Team { get; private set; } = null!;
    public Player Player { get; private set; } = null!;
}

public static class BidExtensions
{
    public static Bid? FindHighestBid(this ICollection<Bid> bids)
        => bids.OrderByDescending(b => b.Amount).FirstOrDefault();
}
