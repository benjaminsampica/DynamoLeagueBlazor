namespace DynamoLeagueBlazor.Shared.Entities;

public record Team : BaseEntity
{
    public Team(string teamKey, string teamName, string teamLogoUrl)
    {
        TeamKey = teamKey;
        TeamName = teamName;
        TeamLogoUrl = teamLogoUrl;
    }

    public string TeamKey { get; set; }
    public string TeamName { get; set; }
    public string TeamLogoUrl { get; set; }

    public ICollection<Player> Players { get; set; } = Array.Empty<Player>();

    public int CapSpace() => Players.Sum(p => p.ContractValue);
}
