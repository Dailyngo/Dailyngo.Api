using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.ControllerCommands.Test.Commands;
using EveryDaily.Application.Services.ControllerCommands.Test.Queries;
using EveryDaily.Core.ControllerBases;
using EveryDaily.Domain.Documents;
using EveryDaily.Persistence.BaseRepositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(IMediator mediator)
    : CustomControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TestModel testModel)
    {
        var response = await mediator.Send(new TestCreateCommand
        {
            TestModel = testModel
        });
        return CreateActionResultInstance(response);
    }

    [HttpGet]
    // [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var response = await mediator.Send(new TestGetAllQuery());
        return CreateActionResultInstance(response);
    }
}