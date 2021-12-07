namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class GetTeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public record TeamItem(
        string Id,
        string TeamLogoUrl,
        string TeamName,
        int PlayerCount,
        int CapSpace);
}
