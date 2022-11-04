namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class AddTeamFineRequest
{
    public int TeamId { get; set; }
    public string FineReason { get; set; } = null!;
    public int Amount { get; set; }
}

public class AddFineRequestValidator : AbstractValidator<AddTeamFineRequest>
{
    public AddFineRequestValidator()
    {
        RuleFor(r => r.TeamId).GreaterThan(0);
        RuleFor(r => r.FineReason).NotEmpty();
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}

public class AddTeamFineRouteFactory
{
    public const string Uri = "api/teams/addfine";
}