﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Players.List;

[Authorize]
[ApiController]
[Route("players")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<PlayerListResult> GetAsync()
    {
        return await _mediator.Send(new Query());
    }
}

public class Query : IRequest<PlayerListResult> { }

public class Handler : IRequestHandler<Query, PlayerListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public Handler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PlayerListResult> Handle(Query request, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players
            .Include(p => p.Team)
            .ProjectTo<PlayerListResult.PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PlayerListResult
        {
            Players = players
        };
    }
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Player, PlayerListResult.PlayerItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team != null ? s.Team.TeamName : string.Empty))
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue.ToString("C0")))
            .ForMember(d => d.HeadShotUrl, mo => mo.MapFrom(s => s.HeadShot));
    }
}
