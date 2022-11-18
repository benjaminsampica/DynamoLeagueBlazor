using DynamoLeagueBlazor.Shared.Enums;
using Microsoft.AspNetCore.WebUtilities;

namespace DynamoLeagueBlazor.Shared.Features.Admin;

public class AddPlayerRequest
{
    public string Name { get; set; } = null!;
    public string Position { get; set; } = null!;
    public int TeamId { get; set; }
    public int ContractValue { get; set; }
}

public class AddPlayerRequestValidator : AbstractValidator<AddPlayerRequest>
{
    public AddPlayerRequestValidator()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Position).NotEmpty()
            .Must((position) => Position.TryFromName(position, out _))
            .WithMessage("Unknown position.");
        RuleFor(p => p.TeamId).GreaterThan(0).WithMessage("Please select a valid team.");
        RuleFor(p => p.ContractValue).GreaterThan(0);
    }
}

public class TeamNameListResult
{
    public IEnumerable<TeamNameItem> Teams { get; init; } = Array.Empty<TeamNameItem>();

    public class TeamNameItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

public class PlayerPreviewRequest
{
    public string Name { get; set; } = null!;
    public string Position { get; set; } = null!;
}

public class PlayerPreviewRequestValidator : AbstractValidator<PlayerPreviewRequest>
{
    public PlayerPreviewRequestValidator()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Position).NotEmpty()
            .Must((position) => Position.TryFromName(position, out _))
            .WithMessage("Unknown position.");
    }
}

public class PlayerPreviewResponse
{
    public string? HeadshotUrl { get; set; }
}

public static class AddPlayerRouteFactory
{
    public const string Uri = "api/admin/addplayer";
    public const string GetTeamListUri = $"{Uri}/teams";
    public const string GetPlayerPreviewUri = $"{Uri}/playerpreview";

    public static string CreatePlayerPreviewUri(string name, string position)
        => QueryHelpers.AddQueryString(GetPlayerPreviewUri, new Dictionary<string, string>()
        {
            { nameof(PlayerPreviewRequest.Name), name },
            { nameof(PlayerPreviewRequest.Position), position }
        });
}