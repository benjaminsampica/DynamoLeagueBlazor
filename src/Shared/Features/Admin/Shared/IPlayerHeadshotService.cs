namespace DynamoLeagueBlazor.Shared.Features.Admin.Shared;

public interface IPlayerHeadshotService
{
    Task<string?> FindPlayerHeadshotUrlAsync(string fullName, string position, CancellationToken cancellationToken);
}
