using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;

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

public class AddFineRouteFactory
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