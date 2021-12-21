namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class FreeAgentListResult
{
    public List<FreeAgentItem> FreeAgents { get; set; }

    public class FreeAgentItem
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string PlayerPosition { get; set; }
        public string PlayerTeam { get; set; }
        public string PlayerHeadShotUrl { get; set; }
        public string BiddingEnds { get; set; }
        public string HighestBid { get; set; }
        public bool CurrentUserIsHighestBidder { get; set; }
    }
}
