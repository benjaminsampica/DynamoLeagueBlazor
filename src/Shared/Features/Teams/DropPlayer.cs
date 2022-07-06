namespace DynamoLeagueBlazor.Shared.Features.Teams;

public class DropPlayerRequest
{
    public int PlayerId { get; set; }
}

public static class DropPlayerRouteFactory
{
    public const string Uri = "api/admin/dropplayer";
}

