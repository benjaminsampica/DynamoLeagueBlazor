using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.OfferMatching;

[ApiController]
[Route(OfferMatchingListRouteFactory.Uri)]
public class ListController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ListController(IMapper mapper, IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<OfferMatchingListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
    [HttpPost]
    public async Task<int> PostAsync([FromBody] MatchPlayerRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<MatchPlayerCommand>(request);
        return await _mediator.Send(query, cancellationToken);
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
            .Where(p => p.TeamId == currentUserTeamId)
            .WhereIsOfferMatching()
            .ProjectTo<OfferMatchingListResult.OfferMatchingItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new OfferMatchingListResult
        {
            OfferMatches = offerMatches
        };
    }
}
public record MatchPlayerCommand(int PlayerId, int Amount) : IRequest<int> { }

public class MatchPlayerHandler : IRequestHandler<MatchPlayerCommand, int>
{
    private readonly ApplicationDbContext _dbContext;

    public MatchPlayerHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(MatchPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players
            .AsTracking()
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken));
        player.ContractValue = request.Amount;
        player.SetToUnsigned();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return player.Id;
    }
    
}
public class ListMappingProfile : Profile
{
    private const int _minimumBid = 1;
    public ListMappingProfile()
    {
        CreateMap<Player, OfferMatchingListResult.OfferMatchingItem>()
            .ForMember(d => d.OfferingTeam, mo => mo.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty))
            .ForMember(d => d.Offer, mo => mo.MapFrom(s =>
                s.Bids.Any()
                ? s.Bids.GetHighestBidder().Amount : _minimumBid)
            );
    }
}

public class MatchPlayerMappingProfile : Profile
{
    public MatchPlayerMappingProfile()
    {
        CreateMap<MatchPlayerRequest, MatchPlayerCommand>();
    }
}
