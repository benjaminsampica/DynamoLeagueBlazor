namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class GetTeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public record TeamItem(
        int Id,
        string TeamLogoUrl,
        string TeamName,
        string PlayerCount,
        string CapSpace);
}
