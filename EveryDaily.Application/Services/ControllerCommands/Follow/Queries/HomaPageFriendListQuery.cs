using EveryDaily.Application.Dtos.Follow;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Queries
{
    public class HomePageFriendListQuery : IRequest<Response<List<HomePageFriendListResponse>>>
    {
        public Guid? UserId { get; set; } 
    }

    public class HomePageFriendListQueryHandler(AppDbContext dbContext, IUserService userService) : IRequestHandler<HomePageFriendListQuery, Response<List<HomePageFriendListResponse>>>
    {
        public async Task<Response<List<HomePageFriendListResponse>>> Handle(HomePageFriendListQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId ?? userService.GetUserId();

            var followedUsers = await dbContext.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                .Select(f => new HomePageFriendListResponse
                {
                    UserId = f.Following.Id,
                    UserName = f.Following.UserName

                })
                .ToListAsync(cancellationToken);

            return Response<List<HomePageFriendListResponse>>.Success(followedUsers, 200);
        }
    }
}