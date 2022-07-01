using DynamoLeagueBlazor.Shared.Features.Dashboard.Shared;

namespace DynamoLeagueBlazor.Shared.Features.Dashboard;

public class TopOffendersResult
{
    public IEnumerable<PlayerItem> Players { get; set; } = Array.Empty<PlayerItem>();

    public class PlayerItem : IRankedItem
    {
        public string ImageUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = null!;
    }
}

public class TopOffendersRouteFactory
{
    public const string Uri = "api/dashboard/topoffenders";
}
