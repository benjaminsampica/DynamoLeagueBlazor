namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class TopOffendersResult
{
    public IEnumerable<PlayerItem> Players { get; set; }

    public class PlayerItem
    {
        public string PlayerHeadShotUrl { get; set; }
        public string PlayerName { get; set; }
        public string FineAmount { get; set; }
    }
}
