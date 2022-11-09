namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public class TeamItem
    {
        public required int Id { get; set; }
        public required string LogoUrl { get; set; }
        public required string Name { get; set; }
        public required string RosteredPlayerCount { get; set; }
        public required string UnrosteredPlayerCount { get; set; }
        public required string UnsignedPlayerCount { get; set; }
        public required string CapSpace { get; set; }
    }
}

public class TeamListRouteFactory
{
    public const string Uri = "api/teams";
}
