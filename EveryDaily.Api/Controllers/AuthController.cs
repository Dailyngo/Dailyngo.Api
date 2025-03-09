using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.ControllerCommands.Auth.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IMediator mediator)
    : CustomControllerBase
{

    /// <summary>
    /// Logs in to the system.
    /// </summary>
    /// <remarks>
    ///Example Request:
    ///
    ///     {
    ///         "emailOrUserName":"admin@dailygno.com",
    ///         "password":"P@ssw0rd"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The login request containing the username or email and password.</param>
    /// <returns>A response indicating success or failure.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            EmailOrUserName = request.EmailOrUserName,
            Password = request.Password
        };
        var result = await mediator.Send(command);
        return CreateActionResultInstance(result);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await mediator.Send(command);
        return CreateActionResultInstance(response);
    }
    
    [HttpGet("email-confirmation")]
    public async Task<IActionResult> EmailConfirmation([FromQuery] string e, [FromQuery] string t)
    {
        var response = await mediator.Send(new EmailVerifyQuery
        {
            Email = e,
            Token = t
        });
        return CreateActionResultInstance(response);
    }
}