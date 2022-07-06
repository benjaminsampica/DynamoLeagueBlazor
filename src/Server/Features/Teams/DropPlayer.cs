using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Teams;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Teams;

[ApiController]
[Route(DropPlayerRouteFactory.Uri)]
public class DropPlayerController : ControllerBase
{
    private readonly IMediator _mediator;

    public DropPlayerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<int> PostAsync(DropPlayerRequest request, CancellationToken cancellationToken)
        => await _mediator.Send(new DropPlayerCommand(request.PlayerId), cancellationToken);
}

public record DropPlayerCommand(int PlayerId) : IRequest<int> { }

public class DropPlayerCommandHandler : IRequestHandler<DropPlayerCommand, int>
{
    private readonly ApplicationDbContext _dbContext;

    public DropPlayerCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(DropPlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players
            .AsTracking()
            .SingleAsync(u => u.Id == command.PlayerId, cancellationToken);

        player.DropFromCurrentTeam();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return player.Id;
    }
}
