using EveryDaily.Application.Dtos.Like.Responses;
using EveryDaily.Application.Services.ControllerCommands.Like.Commands;
using EveryDaily.Application.Services.ControllerCommands.Like.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EveryDaily.Api.Controllers.Post;

[ApiController]
[Route("api/[controller]s")]
[Authorize]
public class LikeController(IMediator mediator) 
    : CustomControllerBase
{
    [HttpPost("{postId}")]
    public async Task<IActionResult> Like(string postId)
    {
        var response = await mediator.Send(new CreateLikeCommand()
        {
            PostId = ObjectId.Parse(postId)
        });

        return CreateActionResultInstance(response);
    }
    
    [HttpDelete("{postId}")]
    public async Task<IActionResult> RemoveLike(string postId)
    {
        var response = await mediator.Send(new RemoveLikeCommand()
        {
            PostId = ObjectId.Parse(postId)
        });

        return CreateActionResultInstance(response);
    }
    
    [HttpGet("{postId}")]
    public async Task<IActionResult> Get(string postId,
        [FromQuery] int pageNumber = 1)
    {
        var response = await mediator.Send(new GetPostLikerListCommand
        {
            PostId = ObjectId.Parse(postId),
            PageNumber = pageNumber
        });
        return CreateActionResultInstance(response);
    }
}