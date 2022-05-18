using System.ComponentModel.DataAnnotations.Schema;

namespace DynamoLeagueBlazor.Server.Models;

public record Fine : BaseEntity
{
    public Fine(decimal amount, string reason, int playerId, int teamId)
    {
        Amount = amount;
        Reason = reason;
        PlayerId = playerId;
        TeamId = teamId;
    }

    [Column(TypeName = "decimal(18,0)")]
    public decimal Amount { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string Reason { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }

    public Player Player { get; private set; } = null!;
    public Team Team { get; private set; } = null!;
}
