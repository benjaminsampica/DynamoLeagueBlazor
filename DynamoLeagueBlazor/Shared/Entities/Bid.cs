namespace DynamoLeagueBlazor.Shared.Entities;

public record Bid : BaseEntity
{
    public Bid(int amount, int teamId, int playerId)
    {
        PlayerId = playerId;
        Amount = amount;
        TeamId = teamId;
    }

    public int Amount { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;

    public Team Team { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
