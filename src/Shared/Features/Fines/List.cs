namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class FineListResult
{
    public IEnumerable<FineItem> Fines { get; init; } = Array.Empty<FineItem>();

    public class FineItem
    {
        public int Id { get; set; }
        public string PlayerHeadShotUrl { get; set; }
        public string PlayerName { get; set; }
        public string TeamName { get; set; }
        public string TeamLogoUrl { get; set; }
        public string Reason { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
    }
}

public class FineListRouteFactory
{
    public const string Uri = "api/fines";
}