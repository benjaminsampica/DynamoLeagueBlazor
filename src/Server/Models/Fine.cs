using System.ComponentModel.DataAnnotations.Schema;

namespace DynamoLeagueBlazor.Server.Models;

public record Fine : BaseEntity
{
    public Fine(decimal amount, string reason, int playerId)
    {
        Amount = amount;
        Reason = reason;
        PlayerId = playerId;
    }

    [Column(TypeName = "decimal(18,0)")]
    public decimal Amount { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string Reason { get; set; }
    public int PlayerId { get; set; }

    public Player Player { get; private set; } = null!;
}
