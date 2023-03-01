using DynamoLeagueBlazor.Shared.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace DynamoLeagueBlazor.Shared.Features.FreeAgents.Detail;

public class AddBidRequest
{
    public int PlayerId { get; set; }
    public int Amount { get; set; }
}

public class AddBidRequestValidator : AsyncAbstractValidator<AddBidRequest>
{
    public AddBidRequestValidator(IBidValidator bidValidator)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PlayerId).GreaterThan(0);
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .MustAsync((request, value, token) => bidValidator.IsHighestAsync(request, token))
            .WithMessage("Please enter a bid higher than the current amount.");
        RuleFor(x => x)
            .MustAsync((request, token) => bidValidator.HasNotEndedAsync(request, token))
            .WithMessage("The bidding has ended.");
    }
}

public interface IBidValidator
{
    public Task<bool> IsHighestAsync(AddBidRequest request, CancellationToken cancellationToken);
    public Task<bool> HasNotEndedAsync(AddBidRequest request, CancellationToken cancellationToken);
}

public static class AddBidRouteFactory
{
    public const string Uri = "api/freeagents/addbid";
    public const string GetIsHighestUri = $"{Uri}/ishighest";
    public const string GetHasNotEndedUri = $"{Uri}/hasnotended";

    public static string Create(string endpoint, AddBidRequest request)
    {
        var uri = QueryHelpers.AddQueryString(endpoint, new Dictionary<string, string>
        {
            { nameof(AddBidRequest.PlayerId), request.PlayerId.ToString() },
            { nameof(AddBidRequest.Amount), request.Amount.ToString() }
        });

        return uri;
    }
}
