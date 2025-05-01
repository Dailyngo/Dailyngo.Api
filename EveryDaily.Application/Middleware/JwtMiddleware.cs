using System.Security.Claims;
using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Services.Badge;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Enums;
using EveryDaily.Domain.Enums.Rank;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Threading;

namespace EveryDaily.Application.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private const string _basePath = "/api/auth";

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, JwtTokenGenerator jwtTokenGenerator,
        UserManager<UserEntity> _userManager, ILogger<JwtMiddleware> logger, IBusControl busControl)
    {
        if (context.Request.Path.ToString().ToLower().Equals($"{_basePath}/login")
            || context.Request.Path.Equals($"{_basePath}/register", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        string? token = null;

        if (context.Request.Path.HasValue && (context.Request.Path.Value.Contains("notification-hub")
                                              || context.Request.Path.Value.Contains("message-hub")))
        {
            token = context.Request.Query["access_token"];
            
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync("Unauthorized");
            }
        }
        else
            token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            await _next(context);
            return;
        }

        var validateTokenResult = await jwtTokenGenerator.VerifyToken(token, JwtTokenType.AccessToken);
        if (validateTokenResult.IsValid)
        {
            context.Items["userid"] = validateTokenResult.UserClaims?
                .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.NameId))?.Value;
            context.Items["email"] = validateTokenResult.UserClaims?
                .FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Email))?.Value;
        }
        else
        {
            logger.LogError(validateTokenResult.Message, new Exception(validateTokenResult.Message));
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = validateTokenResult.Message });
            return;
        }

        await _next(context);
    }
}