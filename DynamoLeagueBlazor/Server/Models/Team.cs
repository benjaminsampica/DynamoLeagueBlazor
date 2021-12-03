namespace DynamoLeagueBlazor.Server.Models;

public record Team : BaseEntity
{
    public Team(string teamKey, string teamName, string teamLogoUrl)
    {
        TeamKey = teamKey;
        TeamName = teamName;
        TeamLogoUrl = teamLogoUrl;
    }

    public string TeamKey { get; init; }
    public string TeamName { get; init; }
    public string TeamLogoUrl { get; init; }

    public ICollection<Player> Players { get; set; } = Array.Empty<Player>();

    public int CapSpace() => Players.Sum(p => p.ContractValue);
}
