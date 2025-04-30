using EveryDaily.Application.Services.ControllerCommands.Statistic;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers;

[ApiController]
[Route("api/[controller]s")]
[Authorize]
public class StatisticController(IMediator mediator)
    : CustomControllerBase
{
    [HttpGet("current")]
    public async Task<IActionResult> GetDailyStatistic()
    {
        var result = await mediator.Send(new CurrentStatisticQuery());
        return CreateActionResultInstance(result);
    }

    // [HttpGet("daily")]
    // public async Task<IActionResult> GetWeeklyStatistic()
    // {
    //     var result = await mediator.Send(new DailyStatisticQuery());
    //     return CreateActionResultInstance(result);
    // }
}