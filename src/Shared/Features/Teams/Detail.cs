namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class TeamDetailResult
{
    public required string LogoUrl { get; set; }
    public required string Name { get; set; }
    public required string CapSpace { get; set; }
    public List<PlayerItem> RosteredPlayers { get; set; } = new List<PlayerItem>();
    public List<PlayerItem> UnrosteredPlayers { get; set; } = new List<PlayerItem>();
    public List<PlayerItem> UnsignedPlayers { get; set; } = new List<PlayerItem>();


    public class PlayerItem
    {
        public required int Id { get; set; }
        public required string HeadShotUrl { get; set; }
        public required string Name { get; set; }
        public required string Position { get; set; }
        public required int ContractValue { get; set; }
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