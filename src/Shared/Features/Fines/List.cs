namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class FineListResult
{
    public IEnumerable<FineItem> Fines { get; init; } = Array.Empty<FineItem>();

    public class FineItem
    {
        public int Id { get; set; }
        public string PlayerHeadShotUrl { get; set; } = null!;
        public string? PlayerName { get; set; }
        public string TeamName { get; set; } = null!;
        public string TeamLogoUrl { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
    }
}

public class FineListRouteFactory
{
    public const string Uri = "api/fines";
}