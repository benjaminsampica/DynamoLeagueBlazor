using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infrastructure.Identity;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(StartSeasonRouteFactory.Uri)]
public class StartSeasonController : ControllerBase
{
    private readonly IMediator _mediator;

    public StartSeasonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<bool> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new IsSeasonStartedQuery(), cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(CancellationToken cancellationToken)
    {
        await _mediator.Send(new StartSeasonCommand(), cancellationToken);

        return NoContent();
    }
}

public record IsSeasonStartedQuery : IRequest<bool> { }

public class IsSeasonStartedHandler : IRequestHandler<IsSeasonStartedQuery, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public IsSeasonStartedHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(IsSeasonStartedQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Players.AnyAsync(p => p.EndOfFreeAgency != null, cancellationToken);
    }
}


public record StartSeasonCommand : IRequest { }

public class StartSeasonHandler : IRequestHandler<StartSeasonCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DateTime _boundaryStartDate = new(DateTime.Today.Year, 6, 25);
    private readonly DateTime _boundaryEndDate = new(DateTime.Today.Year, 8, 20);

    public StartSeasonHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(StartSeasonCommand request, CancellationToken cancellationToken)
    {
        var random = new Random();

        var players = _dbContext.Players
            .Where(p => p.YearContractExpires < DateTime.Today.Year)
            .AsTracking();

        foreach (var player in players)
        {
            var endOfFreeAgency = GetRandomBoundariedDate(random, _boundaryStartDate, _boundaryEndDate);

            player.BeginNewSeason(endOfFreeAgency);
        }

        var startOfTheCurrentYear = new DateTime(DateTime.Today.Year, 1, 1);
        var fines = _dbContext.Fines.Where(f => f.CreatedOn < startOfTheCurrentYear);
        _dbContext.Fines.RemoveRange(fines);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTime GetRandomBoundariedDate(Random seed, DateTime boundaryStartDate, DateTime boundaryEndDate)
    {
        var maximumDaysToAdd = (boundaryEndDate - boundaryStartDate).Days;
        var daysToAdd = seed.Next(maximumDaysToAdd);

        return boundaryStartDate.AddDays(daysToAdd);
    }
}