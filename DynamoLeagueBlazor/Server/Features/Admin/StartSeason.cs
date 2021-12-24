using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Roles = RoleName.Admin)]
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
        await _mediator.Send(new StartSeasonQuery());

        return NoContent();
    }
}

public record StartSeasonQuery : IRequest<Unit> { }

public class StartSeasonHandler : IRequestHandler<StartSeasonQuery>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DateTime _boundaryStartDate = new(DateTime.Today.Year, 6, 25);
    private readonly DateTime _boundaryEndDate = new(DateTime.Today.Year, 8, 20);

    public StartSeasonHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(StartSeasonQuery request, CancellationToken cancellationToken)
    {
        var random = new Random();

        var players = _dbContext.Players
            .WhereIsEligibleForFreeAgency()
            .AsTracking();

        foreach (var player in players)
        {
            var date = GetRandomBoundariedDate(random, _boundaryStartDate, _boundaryEndDate);

            player.SetToFreeAgent(date);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static DateTime GetRandomBoundariedDate(Random seed, DateTime boundaryStartDate, DateTime boundaryEndDate)
    {
        var maximumDaysToAdd = (boundaryEndDate - boundaryStartDate).Days;
        var daysToAdd = seed.Next(maximumDaysToAdd);

        return boundaryStartDate.AddDays(daysToAdd);
    }
}