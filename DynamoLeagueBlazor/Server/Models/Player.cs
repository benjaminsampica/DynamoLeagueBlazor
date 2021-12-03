namespace DynamoLeagueBlazor.Server.Models;

public record Player : BaseEntity
{
    public Player(string playerKey, string name, string position, string headShot)
    {
        PlayerKey = playerKey;
        Name = name;
        Position = position;
        HeadShot = headShot;
    }

    public string PlayerKey { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public int? ContractLength { get; set; }
    public int ContractValue { get; set; }
    public int YearAcquired { get; set; }
    public string HeadShot { get; set; }
    public bool Rostered { get; set; }
    public int? TeamId { get; set; }
    public DateTime? EndOfFreeAgency { get; set; }

    public Team Team { get; private set; } = null!;
    public ICollection<Bid> Bids { get; private set; } = Array.Empty<Bid>();
}
