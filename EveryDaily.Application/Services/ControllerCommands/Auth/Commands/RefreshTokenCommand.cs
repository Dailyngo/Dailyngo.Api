using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Enums;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Commands;

public class RefreshTokenCommand : IRequest<Response<LoginResponse>>
{
    public required string RefreshToken { get; set; }
}

public class RefreshTokenHandler(
    UserManager<UserEntity> userManager,
    JwtTokenGenerator jwtTokenGenerator,
    ILogger<RefreshTokenHandler> logger)
    : IRequestHandler<RefreshTokenCommand, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await jwtTokenGenerator.VerifyToken(request.RefreshToken, JwtTokenType.RefreshToken);

            if (!result.IsValid) return Response<LoginResponse>.Fail("Token not valid", 401);
            var userId = jwtTokenGenerator.GetClaim(request.RefreshToken, JwtRegisteredClaimNames.Sub);

            var user = await userManager.Users.FirstOrDefaultAsync(w => w.Id == Guid.Parse(userId), cancellationToken);

            if (user == null)
                return Response<LoginResponse>.Fail("error.login.invalidcredentials", 401);

            var token = jwtTokenGenerator.GenerateToken(user);
            var refreshToken = await jwtTokenGenerator.GenerateRefreshToken(user);

            return Response<LoginResponse>.Success(new LoginResponse
                { IsSuccess = true, Token = token, RefreshToken = refreshToken, IsRegistered = true });
        }
        catch (Exception ex)
        {
            //logger.SendError(ex, nameof(RefreshTokenCommand));
            return Response<LoginResponse>.Fail("error.unknown", 401);
        }
    }
}