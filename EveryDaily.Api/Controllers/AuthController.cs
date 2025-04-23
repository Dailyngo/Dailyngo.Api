using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Middleware;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.ControllerCommands.Auth.Queries;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
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
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Data = request
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

    [HttpGet("user-login-info")]
    [CustomAuthorize]
    public async Task<IActionResult> GetUserLoginInfo()
    {
        var response = await mediator.Send(new GetUserLoginInfoQuery());
        return CreateActionResultInstance(response);
    }
    
    [HttpGet("send-verification-email")]
    [CustomAuthorize]
    public async Task<IActionResult> SendVerificationEmail()
    {
        var response = await mediator.Send(new SendVerificationEmailCommand());
        return CreateActionResultInstance(response);
    }
    
    [CustomAuthorize]
    [HttpPost("verify-email")]
    public async Task<IActionResult> EmailConfirmation([FromBody] EmailVerifyRequest request)
    {
        var response = await mediator.Send(new EmailVerifyQuery
        {
            VerificationCode = request.VerifyCode
        });
        return CreateActionResultInstance(response);
    }

    [CustomAuthorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordResponse request)
    {
        var command = new UpdatePasswordCommand
        {
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword,
            NewPasswordConfirm = request.NewPasswordConfirm
        };

        var response = await mediator.Send(command);
        return CreateActionResultInstance(response);
    }
}