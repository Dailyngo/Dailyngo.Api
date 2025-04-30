using EveryDaily.Application.Dtos.Report.Request;
using EveryDaily.Application.Services.ControllerCommands.Post.Commands;
using EveryDaily.Application.Services.ControllerCommands.Report.Commands;
using EveryDaily.Application.Services.ControllerCommands.Report.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EveryDaily.Api.Controllers.Post;

[ApiController]
[Route("api/[controller]s")]
[Authorize]
public class ReportController(IMediator mediator) 
    : CustomControllerBase
{
    [HttpPost("post/{postId}")]
    public async Task<IActionResult> ReportPost([FromRoute] string postId, [FromBody] ReportPostRequest request)
    {
        request.PostId = postId;
        var result =  await mediator.Send(new ReportPostCommand()
        {
            Data = request
        });
        
        return CreateActionResultInstance(result);
    }

    [HttpPost("{postId}/setprocess")]
    public async Task<IActionResult> ReportComment([FromRoute] string postId)
    {
        var result = await mediator.Send(new SetPostReportProcessCommand()
        {
            Id = postId
        });
        
        return CreateActionResultInstance(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetPostReports()
    {
        var result = await mediator.Send(new GetPostReportQuery());
        
        return CreateActionResultInstance(result);
    }
    
    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeletePost([FromRoute] string postId)
    {
        var result = await mediator.Send(new DeletePostCommand()
        {
            Id = ObjectId.Parse(postId)
        });
        
        return CreateActionResultInstance(result);
    }
}