namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class FineListResult
{
    public IEnumerable<FineItem> Fines { get; init; } = Array.Empty<FineItem>();

    public class FineItem
    {
        public string PlayerHeadShotUrl { get; set; }
        public string PlayerName { get; set; }
        public string FineReason { get; set; }
        public string FineAmount { get; set; }
        public string FineStatus { get; set; }
    }
}
