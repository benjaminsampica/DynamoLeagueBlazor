namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class GetTeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public class TeamItem
    {
        public int Id { get; set; }
        public string TeamLogoUrl { get; set; }
        public string TeamName { get; set; }
        public string PlayerCount { get; set; }
        public string CapSpace { get; set; }
    }
}
