using System.ComponentModel.DataAnnotations.Schema;

namespace DynamoLeagueBlazor.Server.Models;

public record Fine : BaseEntity
{
    [Column(TypeName = "decimal(18,0)")]
    public required decimal Amount { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public required string Reason { get; set; }
    public required int TeamId { get; set; }
    public int? PlayerId { get; set; }

    public Player? Player { get; private set; }
    public Team Team { get; private set; } = null!;
}
