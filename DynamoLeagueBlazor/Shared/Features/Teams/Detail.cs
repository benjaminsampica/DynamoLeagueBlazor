namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamDetailResult
{
    public string TeamLogoUrl { get; set; }
    public string TeamName { get; set; }
    public string CapSpace { get; set; }

    public IEnumerable<PlayerItem> RosteredPlayers { get; init; } = Array.Empty<PlayerItem>();

    public IEnumerable<PlayerItem> UnrosteredPlayers { get; init; } = Array.Empty<PlayerItem>();

    public class PlayerItem
    {
        public string HeadShotUrl { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string ContractValue { get; set; }
        public string ContractLength { get; set; }
    }
}
