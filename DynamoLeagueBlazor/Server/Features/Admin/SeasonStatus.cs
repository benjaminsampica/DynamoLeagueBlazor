using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route("api/admin/seasonstatus")]
public class SeasonStatusController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeasonStatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<bool> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new SeasonStatusQuery(), cancellationToken);
    }
}

public record SeasonStatusQuery : IRequest<bool> { }

public class SeasonStatusHandler : IRequestHandler<SeasonStatusQuery, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public SeasonStatusHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(SeasonStatusQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Players.AnyAsync(p => p.EndOfFreeAgency != null, cancellationToken);
    }
}
