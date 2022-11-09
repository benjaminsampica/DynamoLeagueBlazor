namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class FineListResult
{
    public IEnumerable<FineItem> Fines { get; init; } = Array.Empty<FineItem>();

    public class FineItem
    {
        public int Id { get; set; }
        public required string PlayerHeadShotUrl { get; set; } = null!;
        public string? PlayerName { get; set; }
        public required string TeamName { get; set; }
        public required string TeamLogoUrl { get; set; }
        public required string Reason { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
    }
}

public class FineListRouteFactory
{
    public const string Uri = "api/fines";
}