using System.Security.Claims;
using EveryDaily.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EveryDaily.Application.Services.UserService;

public interface IUserService
{
    public Guid GetUserId();
    public string? GetUserEmail();
    public Task<bool> IsFollowingAsync(Guid targetUserId, AppDbContext context);
    public Task<bool> IsFollowedByAsync(Guid targetUserId, AppDbContext context);
}

public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public Guid GetUserId()
    {
        if (httpContextAccessor.HttpContext.Items.Any(a => a.Key == "userid"))
        {
            string userId = httpContextAccessor.HttpContext.Items.FirstOrDefault(a => a.Key == "userid").Value
                .ToString();
            Guid userGuid = Guid.Parse(userId);

            return userGuid;
        }
        return Guid.Empty;
    }

    public string? GetUserEmail()
    {
        if (!httpContextAccessor.HttpContext.Items.Any(a => a.Key.Equals("email"))) return null;
        
        var email = httpContextAccessor.HttpContext.Items.FirstOrDefault(a => a.Key.Equals("email")).Value
            ?.ToString();

        return email;
    }

    public async Task<bool> IsFollowingAsync(Guid targetUserId, AppDbContext context)
    {
        var currentUserId = GetUserId();

        var data = await context.Follows
            .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

        return data;
    }

    public async Task<bool> IsFollowedByAsync(Guid targetUserId, AppDbContext context)
    {
        var currentUserId = GetUserId();


        var data = await context.Follows
        .AnyAsync(f => f.FollowerId == targetUserId && f.FollowingId == currentUserId);

        return data;
    }

}