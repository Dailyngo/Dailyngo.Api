using EveryDaily.Application.Dtos.Comment.Requests;
using EveryDaily.Application.Services.ControllerCommands.Comment.Commands;
using EveryDaily.Application.Services.ControllerCommands.Comment.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EveryDaily.Api.Controllers.Post;

[ApiController]
[Route("api/[controller]s")]
[Authorize]
public class CommentController(IMediator mediator) 
    : CustomControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest createCommentRequest)
    {
        var response = await mediator.Send(new CreateCommentCommand
        {
            Data = createCommentRequest
        });
        return CreateActionResultInstance(response);
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> Delete(string commentId)
    {
        var response = await mediator.Send(new DeleteCommentCommand()
        {
            Id = ObjectId.Parse(commentId)
        });
        return CreateActionResultInstance(response);
    }
    
    [HttpGet("{postId}")]
    public async Task<IActionResult> Get(string postId, [FromQuery] int pageNumber = 1)
    {
        var response = await mediator.Send(new GetPostCommentQuery()
        {
            PostId = ObjectId.Parse(postId),
            PageNumber = pageNumber
        });
        return CreateActionResultInstance(response);
    }
}