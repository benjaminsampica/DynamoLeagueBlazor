namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamDetailResult
{
    public string LogoUrl { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string CapSpace { get; set; } = null!;
    public List<PlayerItem> RosteredPlayers { get; set; } = new List<PlayerItem>();
    public List<PlayerItem> UnrosteredPlayers { get; set; } = new List<PlayerItem>();
    public List<PlayerItem> UnsignedPlayers { get; set; } = new List<PlayerItem>();


    public class PlayerItem
    {
        public int Id { get; set; }
        public string HeadShotUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Position { get; set; } = null!;
        public int ContractValue { get; set; }
        public int? YearContractExpires { get; set; }
    }
}

public class TeamDetailRouteFactory
{
    public const string Uri = "api/teams/";

    public static string Create(int teamId)
    {
        return Uri + teamId;
    }
}

public class TeamDetailRequest
{
    public int TeamId { get; set; }
}

public class TeamDetailRequestValidator : AbstractValidator<TeamDetailRequest>
{
    public TeamDetailRequestValidator()
    {
        RuleFor(r => r.TeamId).GreaterThan(0);
    }
}