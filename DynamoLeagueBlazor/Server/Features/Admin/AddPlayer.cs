using AutoMapper;
using DynamoLeagueBlazor.Shared.Features.Players;
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
    }
}
