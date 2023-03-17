using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using System.Text.Json.Serialization;

namespace DynamoLeagueBlazor.Server.Features.Admin.Shared;

public class PlayerHeadshotService : IPlayerHeadshotService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlayerHeadshotService> _logger;

    public PlayerHeadshotService(HttpClient httpClient, ILogger<PlayerHeadshotService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> FindPlayerHeadshotUrlAsync(string fullName, string position, CancellationToken cancellationToken)
    {
        try
        {
            var playerDataResult = await _httpClient.GetFromJsonAsync<PlayerDataResult>(PlayerHeadshotRouteFactory.GetPlayersUri, cancellationToken);

            var matchingPlayers = playerDataResult!.Data.Players
                .Where(p => p.FullName == fullName && p.Position == position)
                .ToList();

            if (matchingPlayers.Count != 1)
            {
                return null;
            }

            var metricResult = await _httpClient.GetFromJsonAsync<PlayerMetricDataResult>(PlayerHeadshotRouteFactory.GetPlayerUri(matchingPlayers.Single().PlayerId!), cancellationToken: cancellationToken);

            return metricResult!.Data.Player.Core?.Avatar;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to acquire player preview.");
            return null;
        }

    }
}

internal class PlayerHeadshotRouteFactory
{
    public const string Uri = "https://www.playerprofiler.com/api/v1";
    public const string GetPlayersUri = Uri + "/players";
    public static string GetPlayerUri(string playerId) => $"{Uri}/player/{playerId}";
}

internal class PlayerDataResult
{
    public required PlayerListResult Data { get; set; }

    public class PlayerListResult
    {
        public IEnumerable<PlayerResult> Players { get; set; } = Array.Empty<PlayerResult>();

        public class PlayerResult
        {
            // Note: This provider has bad data whhere every property is null, thus these needing to be nullable.
            [JsonPropertyName("Player_ID")]
            public string? PlayerId { get; set; }
            [JsonPropertyName("Full Name")]
            public string? FullName { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Position { get; set; }
        }
    }
}

internal class PlayerMetricDataResult
{
    public required PlayerMetricData Data { get; set; }

    public class PlayerMetricData
    {
        public required PlayerData Player { get; set; }

        public class PlayerData
        {
            // Note: This provider has bad data whhere every property is null, thus these needing to be nullable.
            [JsonPropertyName("Player_ID")]
            public string? PlayerId { get; set; }

            public Player? Core { get; set; }

            public class Player
            {
                public string? Avatar { get; set; }
            }
        }
    }
}
