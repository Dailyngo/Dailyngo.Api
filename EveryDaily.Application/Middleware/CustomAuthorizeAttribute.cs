using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EveryDaily.Application.Middleware;

public class CustomAuthorizeAttribute<T> : TypeFilterAttribute where T : Enum
{
    public CustomAuthorizeAttribute(params T[] perms) : base(typeof(CustomAuthorizeFilter))
    {
        Arguments = new object[] { perms };
    }
}
public class CustomAuthorizeAttribute() : TypeFilterAttribute(typeof(CustomAuthorizeFilter));

public class CustomAuthorizeFilter() : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out _))
        {
            context.HttpContext.Response.StatusCode = 401;
            context.HttpContext.Response.StatusCode = 401;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Unauthorized" });
        }

        new AuthorizeAttribute();
    }
}