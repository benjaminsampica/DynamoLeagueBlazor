namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class GetTopOffendersResult
{
    public IEnumerable<PlayerItem> Players { get; set; } = Array.Empty<PlayerItem>();

    public class PlayerItem
    {
        public string PlayerHeadShotUrl { get; set; }
        public string PlayerName { get; set; }
        public string TotalFineAmount { get; set; }
    }
}
