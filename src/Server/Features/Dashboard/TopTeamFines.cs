using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[ApiController]
[Route(TopTeamFinesRouteFactory.Uri)]
public class TopTeamFinesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TopTeamFinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<TopTeamFinesResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new TopTeamFinesQuery(), cancellationToken);
    }
}

public class TopTeamFinesQuery : IRequest<TopTeamFinesResult> { }

public class TopTeamFinesHandler : IRequestHandler<TopTeamFinesQuery, TopTeamFinesResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public TopTeamFinesHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TopTeamFinesResult> Handle(TopTeamFinesQuery request, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.Teams
            .OrderByDescending(p => p.Fines.Sum(f => f.Amount))
            .ProjectTo<TopTeamFinesResult.TeamItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new TopTeamFinesResult
        {
            Teams = teams
        };
    }
}

public class TopTeamFinesMappingProfile : Profile
{
    public TopTeamFinesMappingProfile()
    {
        CreateMap<Team, TopTeamFinesResult.TeamItem>()
            .ForMember(d => d.Amount, mo => mo.MapFrom(s => s.Fines.Sum(f => f.Amount).ToString("C0")))
            .ForMember(d => d.ImageUrl, mo => mo.MapFrom(s => s.LogoUrl));
    }
}
