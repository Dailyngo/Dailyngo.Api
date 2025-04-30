using System.Text.RegularExpressions;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Permissions;
using EveryDaily.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EveryDaily.Application.Middleware;

public class CustomAuthorizeAttribute<T> : TypeFilterAttribute where T : Enum
{
    public CustomAuthorizeAttribute(params T[] perms) : base(typeof(CustomAuthorizeFilter<T>))
    {
        Arguments = new object[] { perms };
    }
}

public class CustomAuthorizeFilter<T>(
    AppDbContext dbContext,
    UserManager<UserEntity> userManager,
    IUserService userService,
    params T[] perms)
    : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out _))
        {
            context.HttpContext.Response.StatusCode = 401;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Unauthorized" });
        }
        var userId = userService.GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            return;
        }
        
        var roles = await userManager.GetRolesAsync(user);
        
        foreach (var perm in perms)
        {
            var permStr = perm.ToString();
            if (roles.Contains(permStr))
            {
                return;
            }
        }
        
        context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
    }
}