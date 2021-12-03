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

    public string PlayerKey { get; private set; }
    public string Name { get; private set; }
    public string Position { get; private set; }
    public string HeadShot { get; private set; }
    public int? ContractLength { get; set; }
    public int ContractValue { get; set; }
    public int YearAcquired { get; set; }
    public bool Rostered { get; set; }
    public int? TeamId { get; set; }
    public DateTime? EndOfFreeAgency { get; set; }

    public Team Team { get; private set; } = null!;
    public ICollection<Bid> Bids { get; private set; } = new HashSet<Bid>();

    public bool IsActive => ContractLength >= DateTime.Now.Year;
}
