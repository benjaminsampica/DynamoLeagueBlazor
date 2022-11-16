using static DynamoLeagueBlazor.Shared.Features.FreeAgents.Detail.FreeAgentDetailResult;

namespace DynamoLeagueBlazor.Client.Features.FreeAgents.Detail;

public partial class BidHistory
{
    [Parameter] public BidItem? OverBid { get; set; }
    [Parameter, EditorRequired] public IEnumerable<BidItem> Bids { get; set; } = new HashSet<BidItem>();
}
