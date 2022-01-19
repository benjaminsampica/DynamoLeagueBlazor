using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Admin.Users;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route("api/admin/users")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<UserListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
}

public class ListQuery : IRequest<UserListResult> { }

public class ListHandler : IRequestHandler<ListQuery, UserListResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public ListHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<UserListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .Include(u => u.Team)
            .ProjectTo<UserListResult.UserItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new UserListResult
        {
            Users = users
        };
    }
}

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<ApplicationUser, UserListResult.UserItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.Name));
    }
}