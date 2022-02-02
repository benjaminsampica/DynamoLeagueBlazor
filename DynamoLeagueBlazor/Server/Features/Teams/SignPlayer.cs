using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Enums;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamDetailResult;

namespace DynamoLeagueBlazor.Server.Features.Teams;

[ApiController]
[Route("api/teams/signplayer")]
public class SignPlayerController : ControllerBase
{
    private readonly IMediator _mediator;

    public SignPlayerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<SignPlayerDetailResult> GetAsync(int playerId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new SignPlayerDetailQuery(playerId), cancellationToken);
    }
    [HttpPost]
    public async Task<int> PostAsync(SignPlayerRequest signPlayerRequest, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new SignPlayerCommand(signPlayerRequest.PlayerId,signPlayerRequest.YearContractExpires), cancellationToken);
    }
}

public record SignPlayerDetailQuery(int PlayerId) : IRequest<SignPlayerDetailResult> { }

public class SignPlayerDetailHandler : IRequestHandler<SignPlayerDetailQuery, SignPlayerDetailResult>
{
    private readonly ApplicationDbContext _dbContext;

    public SignPlayerDetailHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SignPlayerDetailResult> Handle(SignPlayerDetailQuery request, CancellationToken cancellationToken)
    {
       var player = await _dbContext.Players.FindAsync(new object?[] { request.PlayerId }, cancellationToken);
       var position = Position.FromName(player.Position);
       var options = position.CalculateContractYearOptions(player.ContractValue);

        return new SignPlayerDetailResult { ContractOptions = options};
    }

}

public record SignPlayerCommand(int PlayerId,int YearContractExpires) : IRequest<int> { }
public class SignPlayerCommandHandler : IRequestHandler<SignPlayerCommand,int>
{
    private readonly ApplicationDbContext _dbContext;
    public SignPlayerCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<int> Handle(SignPlayerCommand request, CancellationToken cancellationToken)
    {
      var player =   await _dbContext.Players.AsTracking().SingleAsync(u => u.Id == request.PlayerId,cancellationToken);
        var position = Position.FromName(player.Position);
        player.SetToRostered(request.YearContractExpires,position.GetContractValue(player.ContractValue,request.YearContractExpires));
       _dbContext.SaveChanges();
        return player.Id;
    }
}
