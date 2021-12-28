namespace DynamoLeagueBlazor.Shared.Features.Dashboard;

public class TopOffendersResult
{
    public IEnumerable<PlayerItem> Players { get; set; } = Array.Empty<PlayerItem>();

    public class PlayerItem
    {
        public string HeadShotUrl { get; set; }
        public string Name { get; set; }
        public string TotalFineAmount { get; set; }
    }
}
