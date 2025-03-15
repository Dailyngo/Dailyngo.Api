using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EveryDaily.Application.Services.UserService;

public interface IUserService
{
    public Guid GetUserId();
    public string? GetUserEmail();
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
}