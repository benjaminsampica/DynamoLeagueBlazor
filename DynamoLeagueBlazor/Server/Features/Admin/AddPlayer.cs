using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<TeamListResult> GetAsync(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListQuery(), cancellationToken);
        }
    }

    public record ListQuery : IRequest<TeamListResult> { }
}
}
