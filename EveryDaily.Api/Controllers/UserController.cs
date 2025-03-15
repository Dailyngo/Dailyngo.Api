using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Application.Dtos.User.Request;
using EveryDaily.Application.Services.ControllerCommands.About.Commands;
using EveryDaily.Application.Services.ControllerCommands.About.Queries;
using EveryDaily.Application.Services.ControllerCommands.User.Commands;
using EveryDaily.Application.Services.ControllerCommands.User.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers
{



    [ApiController]
    [Route("api/[controller]s")]
    [Authorize]
    public class UserController(IMediator mediator) 
        : CustomControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProfileCardRequest createProfileCardRequest)
        {
            var response = await mediator.Send(new CreateProfileCardCommand
            {
                Data = createProfileCardRequest
            });

            return CreateActionResultInstance(response);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string SearchTerm)
        {
            var response = await mediator.Send(new SearchUsersQuery()
            {
                SearchTerm = SearchTerm
            });
            return CreateActionResultInstance(response);
        }

        [HttpGet("profile-card0")]
        public async Task<IActionResult> Get()
        {
            var response = await mediator.Send(new GetProfileCardQuery());

            return CreateActionResultInstance(response);
        }



    }
}
