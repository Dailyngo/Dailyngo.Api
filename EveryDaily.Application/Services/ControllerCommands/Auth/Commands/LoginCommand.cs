using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Commands;

public class LoginCommand : IRequest<Response<LoginResponse>>
{
    public string EmailOrUserName { get; set; }
    public string Password { get; set; }
}

public class LoginCommandHandler(
    AppDbContext appDbContext,
    SignInManager<UserEntity> SignInManager,
    JwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginCommand, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(x => x.Email == request.EmailOrUserName || x.UserName == request.EmailOrUserName,
            cancellationToken: cancellationToken);

        if (user == null)
            return Response<LoginResponse>.Fail("User not found");

        var result = await SignInManager.PasswordSignInAsync(user, request.Password, false, false);

        if (!result.Succeeded) return Response<LoginResponse>.Fail("Invalid password");
        
        var token = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = await jwtTokenGenerator.GenerateRefreshToken(user);
        return Response<LoginResponse>.Success(new LoginResponse()
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken
        });

    }
}