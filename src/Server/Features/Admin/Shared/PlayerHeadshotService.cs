using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using System.Text.Json.Serialization;

namespace DynamoLeagueBlazor.Server.Features.Admin.Shared;

public class PlayerHeadshotService : IPlayerHeadshotService
{
    private readonly HttpClient _httpClient;
    private ILogger<PlayerHeadshotService> _logger;
    private static PlayerDataResult? _playerDataResult;

    public PlayerHeadshotService(HttpClient httpClient, ILogger<PlayerHeadshotService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> FindPlayerHeadshotUrlAsync(string fullName, string position, CancellationToken cancellationToken)
    {
        if (_playerDataResult == null)
        {
            _playerDataResult = await _httpClient.GetFromJsonAsync<PlayerDataResult>(PlayerHeadshotRouteFactory.GetPlayersUri, cancellationToken);
        }

        var matchingPlayers = _playerDataResult!.Data.Players
            .Where(p => p.FullName == fullName && p.Position == position)
            .ToList();

        if (matchingPlayers.Count != 1)
        {
            return null;
        }

        var metricResult = await _httpClient.GetFromJsonAsync<PlayerMetricDataResult>(PlayerHeadshotRouteFactory.GetPlayerUri(matchingPlayers.Single().PlayerId), cancellationToken: cancellationToken);

        return metricResult!.Data.Player.Core.Avatar;
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
            [JsonPropertyName("Player_ID")]
            public required string PlayerId { get; set; }
            [JsonPropertyName("Full Name")]
            public required string FullName { get; set; }
            public required string Team { get; set; }
            public required string Position { get; set; }
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
            [JsonPropertyName("Player_ID")]
            public required string PlayerId { get; set; }

            public required Player Core { get; set; }

            public class Player
            {
                public required string Avatar { get; set; }
            }
        }
    }
}
