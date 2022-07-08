namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class FreeAgentDetailResult
{
    public string Name { get; set; } = null!;
    public string Position { get; set; } = null!;
    public string HeadShotUrl { get; set; } = null!;
    public string Team { get; set; } = null!;
    public DateTime EndOfFreeAgency { get; set; }

    public IEnumerable<BidItem> Bids { get; set; } = Enumerable.Empty<BidItem>();

    public class BidItem
    {
        public string Team { get; set; } = null!;
        public string Amount { get; set; } = null!;
        public string CreatedOn { get; set; } = null!;
    }
}

public class FreeAgentDetailFactory
{
    public const string Uri = "api/freeagents/";

    public static string Create(int playerId)
    {
        return Uri + playerId;
    }
}