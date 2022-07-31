namespace DynamoLeagueBlazor.Server.Models;

public record Team : BaseEntity
{
    public Team(string name, string logoUrl)
    {
        Name = name;
        LogoUrl = logoUrl;
    }

    public string Name { get; private set; }
    public string LogoUrl { get; private set; }

    public ICollection<Player> Players { get; private set; } = new HashSet<Player>();
    public ICollection<Fine> Fines { get; private set; } = new HashSet<Fine>();
}
