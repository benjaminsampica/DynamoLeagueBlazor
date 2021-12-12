namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamDetailResult
{
    public string TeamLogoUrl { get; set; }
    public string TeamName { get; set; }
    public string CapSpace { get; set; }

    public List<PlayerItem> RosteredPlayers { get; init; } = new List<PlayerItem>();
    public List<PlayerItem> UnrosteredPlayers { get; init; } = new List<PlayerItem>();
    public List<PlayerItem> UnsignedPlayers { get; init; } = new List<PlayerItem>();


    public class PlayerItem
    {
        public string HeadShotUrl { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string ContractValue { get; set; }
        public string ContractLength { get; set; }
    }
}
