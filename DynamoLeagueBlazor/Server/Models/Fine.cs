namespace DynamoLeagueBlazor.Server.Models;

public record Fine : BaseEntity
{
    public Fine(decimal fineAmount, string fineReason, int playerId)
    {
        FineAmount = fineAmount;
        FineReason = fineReason;
        PlayerId = playerId;
    }

    public decimal FineAmount { get; set; }
    public bool Status { get; set; }
    public DateTime FineDate { get; set; } = DateTime.Now;
    public string FineReason { get; set; }
    public int PlayerId { get; set; }

    public Player Player { get; private set; } = null!;
}
