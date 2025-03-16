using EveryDaily.Application.Services.ControllerCommands.Follow.Commands;
using EveryDaily.Application.Services.ControllerQueries.Follow.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController(IMediator mediator)
    : CustomControllerBase
    {
        /// <summary>
        /// Takip isteği gönderir
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> FollowRequest([FromBody] FollowRequestCommand request)
        {
            var result = await mediator.Send(request);
            return CreateActionResultInstance(result);
        }

        /// <summary>
        /// Takip isteği yanıtlar
        /// </summary>
        [HttpPost("answer")]
        public async Task<IActionResult> FollowAnswer([FromBody] HandleFollowRequestCommand request)
        {
            var result = await mediator.Send(request);
            return CreateActionResultInstance(result);
        }

        /// <summary>
        /// Takip - takipçi sayısı getirir
        /// </summary>
        [HttpGet("follow-stats")]
        public async Task<IActionResult> FolloStats([FromQuery] GetUserFollowStatsQuery query)
        {
            var result = await mediator.Send(query);
            return CreateActionResultInstance(result);
        }

        /// <summary>
        /// Takip - takipçi sayısı getirir
        /// </summary>
        [HttpGet("users-list")]
        public async Task<IActionResult> FollowList([FromQuery] GetUserFollowListQuery query)
        {
            var result = await mediator.Send(query);
            return CreateActionResultInstance(result);
        }

        /// <summary>
        /// Takip - takipçi kaldırır
        /// </summary>
        [HttpPost("unfollow")]
        public async Task<IActionResult> UnFollow([FromBody] UnfollowUserCommand query)
        {
            var result = await mediator.Send(query);
            return CreateActionResultInstance(result);
        }
    }
}
