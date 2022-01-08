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
            var command = _mapper.Map<AddPlayerCommand>(request);
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpGet]
        public async Task<TeamNameListResult> GetAsync(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListQuery(), cancellationToken);
        }
    }


}
public record AddPlayerCommand(string Name, string Position,string Headshot,int TeamId,int ContractValue) : IRequest<int> { }

public class AddPlayerHandler : IRequestHandler<AddPlayerCommand, int>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AddPlayerHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<int> Handle(AddPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = new Player(request.Name, request.Position, request.Headshot)
        {
            ContractValue = request.ContractValue,
            TeamId = request.TeamId
        };
        player.SetToUnsigned();
        _dbContext.Add(player);
        _dbContext.SaveChanges();
        return  player.Id;
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
public class AddPlayerMappingProfile : Profile
{
    public AddPlayerMappingProfile()
    {
        CreateMap<AddPlayerRequest, AddPlayerCommand>();
    }
}