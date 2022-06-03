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
