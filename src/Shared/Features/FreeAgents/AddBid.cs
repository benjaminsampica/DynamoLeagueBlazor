using DynamoLeagueBlazor.Shared.Helpers;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;

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

public static class AddBidRouteFactory
{
    public const string Uri = "api/freeagents/addbid";

    public static string Create(AddBidRequest request)
    {
        var uri = QueryHelpers.AddQueryString(Uri, new Dictionary<string, string>
        {
            { nameof(AddBidRequest.PlayerId), request.PlayerId.ToString() },
            { nameof(AddBidRequest.Amount), request.Amount.ToString() }
        });

        return uri;
    }
}
