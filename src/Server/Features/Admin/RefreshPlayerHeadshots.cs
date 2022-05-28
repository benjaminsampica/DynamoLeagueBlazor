using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(RefreshPlayerHeadshotsRouteFactory.Uri)]
public class RefreshPlayerHeadshotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RefreshPlayerHeadshotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RefreshPlayerHeadshotsCommand(), cancellationToken);

        return NoContent();
    }
}

public record RefreshPlayerHeadshotsCommand : IRequest<Unit> { }

public class RefreshPlayerHeadshotsHandler : IRequestHandler<RefreshPlayerHeadshotsCommand>
{
    public const string PlayerProfilerUri = "https://www.playerprofiler.com/api/v1";
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public RefreshPlayerHeadshotsHandler(ApplicationDbContext dbContext, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
    }

    public async Task<Unit> Handle(RefreshPlayerHeadshotsCommand request, CancellationToken cancellationToken)
    {
        var playersUri = $"{PlayerProfilerUri}/players";
        var playerResult = await _httpClient.GetFromJsonAsync<PlayerDataResult>(playersUri, cancellationToken: cancellationToken);

        if (playerResult != null)
        {
            foreach (var player in playerResult.Data.Players)
            {
                var matchingDynamoLeaguePlayer = await _dbContext.Players
                    .AsTracking()
                    .SingleOrDefaultAsync(p =>
                        p.Name == player.FullName
                        && p.Position == player.Position,
                        cancellationToken);

                if (matchingDynamoLeaguePlayer is null) continue;

                var playerUri = $"{PlayerProfilerUri}/player/{player.PlayerId}";
                var metricResult = await _httpClient.GetFromJsonAsync<PlayerMetricDataResult>(playerUri, cancellationToken: cancellationToken);

                matchingDynamoLeaguePlayer.HeadShotUrl = metricResult!.Data.Player.Core.Avatar;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
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
}