using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Commands;

public class LoginCommand : IRequest<Core.Dtos.Response<LoginResponse>>
{
    public string EmailOrUserName { get; set; }
    public string Password { get; set; }
}

public class LoginCommandHandler(
    AppDbContext appDbContext,
    SignInManager<UserEntity> signInManager,
    JwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginCommand, Core.Dtos.Response<LoginResponse>>
{
    public async Task<Core.Dtos.Response<LoginResponse>> Handle(LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(
            x => x.Email == request.EmailOrUserName || x.UserName == request.EmailOrUserName,
            cancellationToken: cancellationToken);

        if (user == null)
            return Core.Dtos.Response<LoginResponse>.Fail(AuthErrorMessage.UserNotFound);

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);

        if (!result.Succeeded) return Core.Dtos.Response<LoginResponse>.Fail(AuthErrorMessage.InvalidPassword);

        var token = await jwtTokenGenerator.GenerateToken(user);
        var refreshToken = await jwtTokenGenerator.GenerateRefreshToken(user);

        return Core.Dtos.Response<LoginResponse>.Success(new LoginResponse()
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken
        });
    }
}