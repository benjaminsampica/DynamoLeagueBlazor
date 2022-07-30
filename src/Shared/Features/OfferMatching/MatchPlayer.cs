namespace DynamoLeagueBlazor.Shared.Features.OfferMatching;

public record MatchPlayerRequest(int PlayerId);

public class MatchPlayerRequestValidator : AbstractValidator<MatchPlayerRequest>
{
    public MatchPlayerRequestValidator(IMatchPlayerValidator offerMatchingValidator)
    {
        RuleFor(x => x.PlayerId)
            .GreaterThan(0)
            .MustAsync(async (request, value, token) => await offerMatchingValidator.CanOfferMatchAsync(value, token));
    }
}

public static class MatchPlayerRouteFactory
{
    public const string Uri = "api/matchplayer";
}

public interface IMatchPlayerValidator
{
    Task<bool> CanOfferMatchAsync(int playerId, CancellationToken cancellationToken);
}
