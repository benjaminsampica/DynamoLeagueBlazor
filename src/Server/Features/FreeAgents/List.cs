﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

[ApiController]
[Route(FreeAgentListRouteFactory.Uri)]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<FreeAgentListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
}

public record ListQuery : IRequest<FreeAgentListResult> { }

public class ListHandler : IRequestHandler<ListQuery, FreeAgentListResult>
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

    public async Task<FreeAgentListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var currentUserTeamId = _httpContextAccessor.HttpContext!.User.GetTeamId();

        var freeAgents = await _dbContext.Players
            .Include(p => p.Team)
            .Include(p => p.Bids)
                .ThenInclude(b => b.Team)
            .Where(p => p.State == PlayerState.FreeAgent)
            .OrderBy(p => p.EndOfFreeAgency)
            .ProjectTo<FreeAgentListResult.FreeAgentItem>(_mapper.ConfigurationProvider, new { currentUserTeamId })
            .ToListAsync(cancellationToken);

        return new FreeAgentListResult
        {
            FreeAgents = freeAgents
        };
    }
}

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        int currentUserTeamId = 0;

        CreateMap<Player, FreeAgentListResult.FreeAgentItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty))
            .ForMember(d => d.CurrentUserIsHighestBidder, mo => mo.MapFrom(s =>
                s.Bids.Any() &&
                s.Bids.FindHighestBid()!.TeamId == currentUserTeamId)
            )
            .ForMember(d => d.BiddingEnds, mo => mo.MapFrom(s => s.EndOfFreeAgency!.Value))
            .ForMember(d => d.HighestBid, mo => mo.MapFrom(s => s.GetHighestBidAmount()))
            .ForMember(d => d.WinningTeam, mo => mo.MapFrom(s => s.Bids.Any() ? s.Bids.FindHighestBid()!.Team.Name : null));
    }
}
