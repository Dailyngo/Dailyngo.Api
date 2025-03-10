using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.ControllerCommands.About.Commands;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers
{



    [ApiController]
    [Route("api/[controller]s")]
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
    }
}
