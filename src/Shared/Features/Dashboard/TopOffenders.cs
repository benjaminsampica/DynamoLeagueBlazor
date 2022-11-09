using DynamoLeagueBlazor.Shared.Features.Dashboard.Shared;

namespace DynamoLeagueBlazor.Shared.Features.Dashboard;

public class TopOffendersResult
{
    public IEnumerable<PlayerItem> Players { get; set; } = Array.Empty<PlayerItem>();

    public class PlayerItem : IRankedItem
    {
        public required string ImageUrl { get; set; }
        public required string Name { get; set; }
        public required string Amount { get; set; }
    }
}

public class TopOffendersRouteFactory
{
    public const string Uri = "api/dashboard/topoffenders";
}
