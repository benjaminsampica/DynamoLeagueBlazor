using FluentValidation;

namespace DynamoLeagueBlazor.Shared.Features.Fines;

public record AddFineRequest(int PlayerId, string FineReason);

public class AddFineRequestValidator : AbstractValidator<AddFineRequest>
{
    public AddFineRequestValidator()
    {
        RuleFor(r => r.PlayerId).GreaterThan(0);
        RuleFor(r => r.FineReason).NotEmpty();
    }
}