namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public class TeamItem
    {
        public int Id { get; set; }
        public string LogoUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string RosteredPlayerCount { get; set; } = null!;
        public string UnrosteredPlayerCount { get; set; } = null!;
        public string UnsignedPlayerCount { get; set; } = null!;
        public string CapSpace { get; set; } = null!;
    }
}

public class TeamListRouteFactory
{
    public const string Uri = "api/teams";
}
