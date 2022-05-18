using DynamoLeagueBlazor.Shared.Features.Dashboard.Shared;

namespace DynamoLeagueBlazor.Shared.Features.Dashboard;

public class TopTeamFinesResult
{
    public IEnumerable<TeamItem> Teams { get; set; } = Array.Empty<TeamItem>();

    public class TeamItem : IRankedItem
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
    }
}

public class TopTeamFinesRouteFactory
{
    public const string Uri = "api/dashboard/topteamfines";
}
