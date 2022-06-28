namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class FreeAgentListResult
{
    public List<FreeAgentItem> FreeAgents { get; set; }

    public class FreeAgentItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public string HeadShotUrl { get; set; }
        public DateTime BiddingEnds { get; set; }
        public int HighestBid { get; set; }
        public bool CurrentUserIsHighestBidder { get; set; }
    }
}

public class FreeAgentListRouteFactory
{
    public const string Uri = "api/freeagents";
}
