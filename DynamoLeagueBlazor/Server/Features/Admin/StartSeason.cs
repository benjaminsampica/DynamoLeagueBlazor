using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Roles = ApplicationRole.Admin)]
[ApiController]
[Route("admin/startseason")]
public class StartSeasonController : ControllerBase
{
    private readonly IMediator _mediator;

    public StartSeasonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync()
    {
        await _mediator.Send(new Query());

        return NoContent();
    }
}

public record Query : IRequest<Unit> { }

public class Handler : IRequestHandler<Query>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DateTime _boundaryStartDate = new(DateTime.Today.Year, 6, 25);
    private readonly DateTime _boundaryEndDate = new(DateTime.Today.Year, 8, 20);

    public Handler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
    {
        var random = new Random();

        var players = _dbContext.Players
            .WhereIsAFreeAgent()
            .AsTracking();

        foreach (var player in players)
        {
            player.EndOfFreeAgency = GetRandomBoundariedEndOfFreeAgency(random, _boundaryStartDate, _boundaryEndDate);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static DateTime GetRandomBoundariedEndOfFreeAgency(Random seed, DateTime boundaryStartDate, DateTime boundaryEndDate)
    {
        var maximumDaysToAdd = (boundaryEndDate - boundaryStartDate).Days;
        var daysToAdd = seed.Next(maximumDaysToAdd);

        return boundaryStartDate.AddDays(daysToAdd);
    }
}