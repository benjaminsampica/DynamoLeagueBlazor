using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamNameListResult;

namespace DynamoLeagueBlazor.Server.Features.Admin
{
    [Authorize(Roles = RoleName.Admin)]
    [ApiController]
    [Route("admin/addplayer")]
    public class AddPlayerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;

        public AddPlayerController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<int> PostAsync([FromBody] AddPlayerRequest request, CancellationToken cancellationToken)
        {


            return 1;
        }

        [HttpGet]
        public async Task<TeamNameListResult> GetAsync(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListQuery(), cancellationToken);
        }
    }


}
public record ListQuery : IRequest<TeamNameListResult> { }
public class ListHandler : IRequestHandler<ListQuery, TeamNameListResult>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(IDbContextFactory<ApplicationDbContext> dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TeamNameListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        List<TeamNameItem> teams = new();
        using (var dbContext = await _dbContext.CreateDbContextAsync(cancellationToken))
        {
            teams = await dbContext.Teams
                .ProjectTo<TeamNameItem>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        };

        return new TeamNameListResult
        {
            Teams = teams
        };
    }
}
public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Team, TeamNameItem>();
    }
}