namespace DynamoLeagueBlazor.Shared.Features.Fines;

public class ManageFineRequest
{
    public int FineId { get; set; }
    public bool Approved { get; set; }
}

public class ManageFineRequestValidator : AbstractValidator<ManageFineRequest>
{
    public ManageFineRequestValidator()
    {
        RuleFor(p => p.FineId).GreaterThan(0);
    }
}

public class ManageFineRouteFactory
{
    public const string Uri = "api/fines/manage";
}