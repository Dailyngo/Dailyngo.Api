using System.Text.Json;
using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Auth;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Queries;

public class GetUserLoginInfoQuery : IRequest<Core.Dtos.Response<LoginDetailResponse>>;

public class GetUserLoginInfoQueryHandler(
    AppDbContext appDbContext,
    IUserService userService)
    : IRequestHandler<GetUserLoginInfoQuery, Core.Dtos.Response<LoginDetailResponse>>
{
    public async Task<Core.Dtos.Response<LoginDetailResponse>> Handle(GetUserLoginInfoQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();
        var user = await appDbContext.Users.FirstOrDefaultAsync(
            x => x.Id == userId && !x.IsDeleted, cancellationToken);

        var userAboutExist = await appDbContext.Abouts.AnyAsync(x => x.UserId == user.Id, cancellationToken);

        return Core.Dtos.Response<LoginDetailResponse>.Success(new LoginDetailResponse()
        {
            IsRegistered = userAboutExist,
            IsEmailConfirmed = user.EmailConfirmed
        });
    }
}