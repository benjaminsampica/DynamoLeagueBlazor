using DynamoLeagueBlazor.Shared.Enums;
using FluentValidation;
namespace DynamoLeagueBlazor.Shared.Features.Admin;

public class AddPlayerRequest
{
    public string Name { get; set; }
    public string Position { get; set; }
    public string HeadShot { get; set; }
    public int TeamId { get; set; }
    public int ContractValue { get; set; }
}
public class AddPlayerRequestValidator : AbstractValidator<AddPlayerRequest>
{
    public AddPlayerRequestValidator()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Position).NotEmpty().Must((position) => Position.TryFromName(position, out _));
        RuleFor(p => p.HeadShot).NotEmpty();
        RuleFor(p => p.TeamId).GreaterThan(0).WithMessage("Please select a valid team.");
        RuleFor(p => p.ContractValue).GreaterThan(0);
    }
}

public static class AddPlayerRouteFactory
{
    public const string Uri = "api/admin/addplayer";
}