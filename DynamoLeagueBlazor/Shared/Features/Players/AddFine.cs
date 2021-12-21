using FluentValidation;

namespace DynamoLeagueBlazor.Shared.Features.Players;

public class AddFineRequest
{
    public int PlayerId { get; set; }
    public string FineReason { get; set; }
}

public class AddFineRequestValidator : AbstractValidator<AddFineRequest>
{
    public AddFineRequestValidator()
    {
        RuleFor(r => r.PlayerId).GreaterThan(0);
        RuleFor(r => r.FineReason).NotEmpty();
    }
}

public class FineDetailResult
{
    public int PlayerId { get; set; }
    public string ContractValue { get; set; } = "Unknown";
    public string FineAmount { get; set; } = "Unknown";
}

public class FineDetailRequest
{
    public int PlayerId { get; set; }
}

public class FineDetailRequestValidator : AbstractValidator<FineDetailRequest>
{
    public FineDetailRequestValidator()
    {
        RuleFor(r => r.PlayerId).GreaterThan(0);
    }
}
