using EveryDaily.Application.Services.ControllerCommands.Message.Queries;
using EveryDaily.Core.ControllerBases;
using EveryDaily.Domain.Documents;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EveryDaily.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MessageController(
    IMediator mediator)
    : CustomControllerBase
{
    /// <summary>
    /// Son mesajlasilmis kisileri getirir
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetMessagesUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 40)
    {
        var result = await mediator.Send(new GetMessagesUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return CreateActionResultInstance(result);
    }
    
    /// <summary>
    /// Son mesajlasilmis kisilerle olan mesajlari getirir
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetMessages([FromRoute] Guid userId, [FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 40)
    {
        var result = await mediator.Send(new GetMessagesQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return CreateActionResultInstance(result);
    }
}