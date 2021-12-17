namespace DynamoLeagueBlazor.Server.Models;

public record Team : BaseEntity
{
    public Team(string teamName, string teamLogoUrl)
    {
        TeamName = teamName;
        TeamLogoUrl = teamLogoUrl;
    }

    public string TeamName { get; private set; }
    public string TeamLogoUrl { get; private set; }

    public ICollection<Player> Players { get; private set; } = new HashSet<Player>();

    public int CapSpace() => Players.Sum(p => p.ContractValue);
}
