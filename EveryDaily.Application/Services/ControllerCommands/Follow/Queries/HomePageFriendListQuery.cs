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
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class HomePageFriendListQueryHandler(AppDbContext dbContext, IUserService userService) : IRequestHandler<HomePageFriendListQuery, Response<List<HomePageFriendListResponse>>>
    {
        public async Task<Response<List<HomePageFriendListResponse>>> Handle(HomePageFriendListQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId ?? userService.GetUserId();

            var followedUsers = await dbContext.Follows
                .Include(f => f.Following)
                .Where(f => f.FollowerId == userId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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