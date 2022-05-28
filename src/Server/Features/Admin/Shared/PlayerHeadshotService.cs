using System.Text.Json.Serialization;

namespace DynamoLeagueBlazor.Server.Features.Admin.Shared;

internal class PlayerHeadshotService
{



}

internal class PlayerProfilerRouteFactory
{
    public const string Uri = "https://www.playerprofiler.com/api/v1";
    public const string GetPlayersUri = Uri + "/players";
    public static string GetPlayerUri(string playerId) => $"{Uri}/player/{playerId}";
}

internal class PlayerDataResult
{
    public PlayerListResult Data { get; set; } = null!;

    public class PlayerListResult
    {
        public IEnumerable<PlayerResult> Players { get; set; } = Array.Empty<PlayerResult>();

        public class PlayerResult
        {
            [JsonPropertyName("Player_ID")]
            public string PlayerId { get; set; } = null!;
            [JsonPropertyName("Full Name")]
            public string FullName { get; set; } = null!;
            public string Team { get; set; } = null!;
            public string Position { get; set; } = null!;
        }
    }
}

internal class PlayerMetricDataResult
{
    public PlayerMetricData Data { get; set; } = null!;

    public class PlayerMetricData
    {
        public PlayerData Player { get; set; } = null!;

        public class PlayerData
        {
            [JsonPropertyName("Player_ID")]
            public string PlayerId { get; set; } = null!;

            public Player Core { get; set; } = null!;

            public class Player
            {
                public string Avatar { get; set; } = null!;
            }
        }
    }
}
