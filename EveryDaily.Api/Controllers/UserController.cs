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
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string SearchTerm)
        {
            var response = await mediator.Send(new SearchUsersQuery()
            {
                SearchTerm = SearchTerm
            });
            return CreateActionResultInstance(response);
        }

        [HttpGet("profile-card")]
        public async Task<IActionResult> Get()
        {
            var response = await mediator.Send(new GetProfileCardQuery());

            return CreateActionResultInstance(response);
        }

        [HttpGet("birthdays")]
        public async Task<IActionResult> GetTodaysBirthdays()
        {
           
            var result = await mediator.Send(new GetBirthdayListQuery());
            return CreateActionResultInstance(result);
        }
    }
}