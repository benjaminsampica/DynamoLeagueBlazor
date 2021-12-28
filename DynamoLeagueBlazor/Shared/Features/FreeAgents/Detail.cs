namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class FreeAgentDetailResult
{
    public string Name { get; set; }
    public string Position { get; set; }
    public string HeadShotUrl { get; set; }
    public string Team { get; set; }
    public string EndOfFreeAgency { get; set; }

    public IEnumerable<BidItem> Bids { get; set; } = Enumerable.Empty<BidItem>();

    public class BidItem
    {
        public string Team { get; set; }
        public string Amount { get; set; }
        public string CreatedOn { get; set; }
    }
}