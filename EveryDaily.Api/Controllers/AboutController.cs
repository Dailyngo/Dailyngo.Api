using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Application.Dtos.About.Response;
using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.ControllerCommands.About.Commands;
using EveryDaily.Application.Services.ControllerCommands.About.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers
{



    [ApiController]
    [Route("api/[controller]s")]
    [Authorize]
    public class AboutController(IMediator mediator) 
        : CustomControllerBase
    {
        [HttpPost]
       
        public async Task<IActionResult> Create([FromBody] CreateAboutRequest createAboutRequest)
        {
            var response = await mediator.Send(new CreateAboutCommand
            {
                Data = createAboutRequest
            });

            return CreateActionResultInstance(response);
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAboutRequest updateAboutRequest)
        {
            var response = await mediator.Send(new UpdateAboutCommand
            {
                Data = updateAboutRequest
            });

            return CreateActionResultInstance(response);
        }

        [HttpGet("about")]
        public async Task<IActionResult> Get()
        {
            var response = await mediator.Send(new GetAboutQuery());

            return CreateActionResultInstance(response);
        }

        [Authorize]
        [HttpGet("other-about/{userId}")]
        public async Task<IActionResult> OtherGet(Guid userId)
        {
            var response = await mediator.Send(new GetOtherUserAboutQuery(userId));
            return CreateActionResultInstance(response);
        }


    }
}
