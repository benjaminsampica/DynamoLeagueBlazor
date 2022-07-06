using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Server.Features.OfferMatching;

[ApiController]
[Route(OfferMatchingListRouteFactory.Uri)]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<OfferMatchingListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
    [HttpPost]
    public async Task<Unit> PostAsync([FromBody] MatchPlayerRequest request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new MatchPlayerCommand(request.PlayerId), cancellationToken);
    }
}

public record ListQuery : IRequest<OfferMatchingListResult> { }

public class ListHandler : IRequestHandler<ListQuery, OfferMatchingListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ListHandler(
        ApplicationDbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OfferMatchingListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var currentUserTeamId = _httpContextAccessor.HttpContext!.User.GetTeamId();

        var offerMatches = await _dbContext.Players
            .Include(p => p.Bids)
            .Where(p => p.TeamId == currentUserTeamId
                && p.State == PlayerState.OfferMatching)
            .ProjectTo<OfferMatchingListResult.OfferMatchingItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new OfferMatchingListResult
        {
            OfferMatches = offerMatches
        };
    }
}
public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Player, OfferMatchingListResult.OfferMatchingItem>()
            .ForMember(d => d.OfferingTeam, mo => mo.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty))
            .ForMember(d => d.Offer, mo => mo.MapFrom(s => s.GetHighestBidAmount()));
    }
}
public record MatchPlayerCommand(int PlayerId) : IRequest { }

public class MatchPlayerHandler : IRequestHandler<MatchPlayerCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public MatchPlayerHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(MatchPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players
            .AsTracking()
            .Include(p => p.Bids)
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken));

        player.MatchOffer();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

}
