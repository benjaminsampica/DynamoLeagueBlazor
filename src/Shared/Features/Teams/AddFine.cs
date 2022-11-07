namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class AddFineRequest
{
    public int TeamId { get; set; }
    public string FineReason { get; set; } = null!;
    public int Amount { get; set; }
}

public class AddFineRequestValidator : AbstractValidator<AddFineRequest>
{
    public AddFineRequestValidator()
    {
        RuleFor(r => r.TeamId).GreaterThan(0);
        RuleFor(r => r.FineReason).NotEmpty();
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}

public class AddFineRouteFactory
{
    public const string Uri = "api/teams/addfine";
}