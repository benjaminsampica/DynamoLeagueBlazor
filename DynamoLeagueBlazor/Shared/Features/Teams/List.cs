namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public class TeamItem
    {
        public int Id { get; set; }
        public string TeamLogoUrl { get; set; }
        public string TeamName { get; set; }
        public string RosteredPlayerCount { get; set; }
        public string UnrosteredPlayerCount { get; set; }
        public string UnsignedPlayerCount { get; set; }
        public string CapSpace { get; set; }
    }
}
