using EveryDaily.Application.Dtos.Post.Requests;
using EveryDaily.Application.Services.ControllerCommands.Post.Commands;
using EveryDaily.Application.Services.ControllerCommands.Post.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EveryDaily.Api.Controllers.Post;

[ApiController]
[Route("api/[controller]s")]
[Authorize]
public class PostController(IMediator mediator)
    : CustomControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest createPostRequest)
    {
        var response = await mediator.Send(new CreatePostCommand
        {
            Data = createPostRequest
        });
        return CreateActionResultInstance(response);
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> Delete(string postId)
    {
        var response = await mediator.Send(new DeletePostCommand()
        {
            Id = ObjectId.Parse(postId)
        });
        return CreateActionResultInstance(response);
    }
    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? userId, [FromQuery] int pageNumber = 1)
    {
        var response = await mediator.Send(new GetUserPostQuery
        {
            UserId = userId,
            PageNumber = pageNumber
        });
        return CreateActionResultInstance(response);
    }
    
    [HttpGet("homepage")]
    public async Task<IActionResult> GetHomePage([FromQuery] int pageNumber = 1)
    {
        var response = await mediator.Send(new GetHomePagePostQuery
        {
            PageNumber = pageNumber
        });
        return CreateActionResultInstance(response);
    }

    [HttpGet("byId")]
    public async Task<IActionResult> Get([FromQuery] GetPostByIdQuery query)
    {
        var response = await mediator.Send(query);
        return CreateActionResultInstance(response);
    }
}