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

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Player, OfferMatchingListResult.OfferMatchingItem>()
            .ForMember(d => d.OfferingTeam, mo => mo.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty))
            .ForMember(d => d.Offer, mo => mo.MapFrom(s =>
                s.Bids.Any()
                ? s.Bids.GetHighestBidder().Amount.ToString("C0")
                : string.Empty)
            );
    }
}
