namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class FreeAgentListResult
{
    public List<FreeAgentItem> FreeAgents { get; set; } = new();

    public class FreeAgentItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string Team { get; set; } = null!;
        public string HeadShotUrl { get; set; } = null!;
        public DateTime BiddingEnds { get; set; }
        public int HighestBid { get; set; }
        public bool CurrentUserIsHighestBidder { get; set; }
    }
}

public class FreeAgentListRouteFactory
{
    public const string Uri = "api/freeagents";
}
