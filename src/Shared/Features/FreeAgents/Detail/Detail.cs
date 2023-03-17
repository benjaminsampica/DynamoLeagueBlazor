namespace DynamoLeagueBlazor.Shared.Features.FreeAgents.Detail;

public class FreeAgentDetailResult
{
    public required string Name { get; set; }
    public required string Position { get; set; }
    public required string HeadShotUrl { get; set; }
    public required string Team { get; set; }
    public DateTimeOffset EndOfFreeAgency { get; set; }

    public BidItem? OverBid { get; set; }

    public IEnumerable<BidItem> Bids { get; set; } = Enumerable.Empty<BidItem>();

    public class BidItem
    {
        public required string Team { get; set; }
        public required int Amount { get; set; }
        public required string CreatedOn { get; set; }
    }
}

public class FreeAgentDetailFactory
{
    public const string Uri = "api/freeagents/";

    public static string Create(int playerId) => Uri + playerId;
}