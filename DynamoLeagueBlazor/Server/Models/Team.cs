namespace DynamoLeagueBlazor.Server.Models;

public record Team : BaseEntity
{
    public Team(string teamKey, string teamName, string teamLogoUrl)
    {
        TeamKey = teamKey;
        TeamName = teamName;
        TeamLogoUrl = teamLogoUrl;
    }

    public string TeamKey { get; private set; }
    public string TeamName { get; private set; }
    public string TeamLogoUrl { get; private set; }

    public ICollection<Player> Players { get; private set; } = new HashSet<Player>();

    public int CapSpace() => Players.Sum(p => p.ContractValue);
}
