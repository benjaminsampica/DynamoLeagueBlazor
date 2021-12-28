using DynamoLeagueBlazor.Shared.Helpers;
using FluentValidation;

namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class AddBidRequest
{
    public int PlayerId { get; set; }
    public int Amount { get; set; }
}

public class AddBidRequestValidator : AsyncAbstractValidator<AddBidRequest>
{
    public AddBidRequestValidator(IBidAmountValidator bidAmountValidator)
    {
        RuleFor(x => x.PlayerId).GreaterThan(0);
        RuleFor(x => x.Amount)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .MustAsync((request, value, context, token) => bidAmountValidator.IsHighestBidAsync(request, token))
            .WithMessage("Please enter a bid higher than the current amount.");
    }
}

public interface IBidAmountValidator
{
    public Task<bool> IsHighestBidAsync(AddBidRequest request, CancellationToken cancellationToken);
}
