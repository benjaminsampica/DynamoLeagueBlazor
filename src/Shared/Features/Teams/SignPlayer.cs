using DynamoLeagueBlazor.Shared.Enums;
using Microsoft.AspNetCore.WebUtilities;

namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class SignPlayerRequest
{
    public int PlayerId { get; set; }
    public int YearContractExpires { get; set; }
}

public class SignPlayerRequestValidator : AbstractValidator<SignPlayerRequest>
{
    public SignPlayerRequestValidator()
    {
        RuleFor(r => r.PlayerId).GreaterThan(0);
        RuleFor(r => r.YearContractExpires).GreaterThan(0);
    }
}

public class SignPlayerDetailRequest
{
    public int PlayerId { get; set; }
}

public class SignPlayerDetailResult
{
    public IEnumerable<ContractOption> ContractOptions { get; set; } = Enumerable.Empty<ContractOption>();
}

public static class SignPlayerRouteFactory
{
    public const string Uri = "api/admin/signplayer";

    public static string Create(int playerId)
        => QueryHelpers.AddQueryString(Uri, nameof(SignPlayerRequest.PlayerId), playerId.ToString());
}
