namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamDetailResult
{
    public string LogoUrl { get; set; }
    public string Name { get; set; }
    public string CapSpace { get; set; }
    public List<PlayerItem> RosteredPlayers { get; set; } = new List<PlayerItem>();
    public List<PlayerItem> UnrosteredPlayers { get; set; } = new List<PlayerItem>();
    public List<PlayerItem> UnsignedPlayers { get; set; } = new List<PlayerItem>();


    public class PlayerItem
    {
        public int Id { get; set; }
        public string HeadShotUrl { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string ContractValue { get; set; }
        public int ContractValueAmount { get; set; }
        public string YearContractExpires { get; set; }
        public DateTime YearContractExpiresDate { get; set; }
    }
}

public class TeamDetailRouteFactory
{
    public const string Uri = "api/teams/";

    // TODO: change to request and add validator + tests
    // see FineDetailRequest
    public static string Create(int teamId)
    {
        return Uri + teamId;
    }
}