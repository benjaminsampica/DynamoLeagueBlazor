using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamoLeagueBlazor.Server.Features.Players;

[Authorize]
[ApiController]
[Route("players/finedetail")]
public class FineDetailController : ControllerBase
{
    private readonly IMediator _mediator;

    public FineDetailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<FineDetailResult> GetAsync([FromQuery] FineDetailRequest request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new FineDetailQuery(request.PlayerId), cancellationToken);
    }
}

public record FineDetailQuery(int PlayerId) : IRequest<FineDetailResult> { }

public class FineDetailHandler : IRequestHandler<FineDetailQuery, FineDetailResult>
{
    private readonly ApplicationDbContext _dbContext;

    public FineDetailHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FineDetailResult> Handle(FineDetailQuery request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players.FindAsync(new object?[] { request.PlayerId }, cancellationToken);

        var result = new FineDetailResult
        {
            PlayerId = player!.Id,
            ContractValue = player.ContractValue.ToString("C0"),
            FineAmount = FineUtilities.CalculateFineAmount(player.ContractValue).ToString("C0")
        };

        return result;
    }
}