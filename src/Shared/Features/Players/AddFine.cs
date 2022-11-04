using Microsoft.AspNetCore.WebUtilities;

namespace DynamoLeagueBlazor.Shared.Features.Players;

public class AddPlayerFineRequest
{
    public int PlayerId { get; set; }
    public string FineReason { get; set; } = null!;
}

public class AddFineRequestValidator : AbstractValidator<AddPlayerFineRequest>
{
    public AddFineRequestValidator()
    {
        RuleFor(r => r.PlayerId).GreaterThan(0);
        RuleFor(r => r.FineReason).NotEmpty();
    }
}

public class AddPlayerFineRouteFactory
{
    public const string Uri = "api/players/addfine";
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

public class FineDetailRouteFactory
{
    public const string Uri = "api/players/finedetail";

    public static string Create(int playerId)
    {
        return QueryHelpers.AddQueryString(Uri, nameof(FineDetailRequest.PlayerId), playerId.ToString());
    }
}