using Microsoft.AspNetCore.WebUtilities;

namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class DropPlayerRequest
{
    public int PlayerId { get; set; }
}

public static class DropPlayerRouteFactory
{
    public const string Uri = "api/admin/dropplayer";

    public static string Create(int playerId)
        => QueryHelpers.AddQueryString(Uri, nameof(DropPlayerRequest.PlayerId), playerId.ToString());
}

