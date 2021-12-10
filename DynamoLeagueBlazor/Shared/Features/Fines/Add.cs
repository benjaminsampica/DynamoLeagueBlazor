using FluentValidation;

namespace DynamoLeagueBlazor.Shared.Features.Fines;

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