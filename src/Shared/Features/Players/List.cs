namespace DynamoLeagueBlazor.Shared.Features.Players;

public class PlayerListResult
{
    public IEnumerable<PlayerItem> Players { get; init; } = Array.Empty<PlayerItem>();

    public class PlayerItem
    {
        public int Id { get; set; }
        public string HeadShotUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string Team { get; set; } = null!;
        public int ContractValue { get; set; }
        public int YearContractExpires { get; set; }
    }
}

public class PlayerListRouteFactory
{
    public const string Uri = "api/players";
}