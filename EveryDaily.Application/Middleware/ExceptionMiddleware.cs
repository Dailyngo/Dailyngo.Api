using System.Net;
using EveryDaily.Core.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EveryDaily.Application.Middleware;

public class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context,ILogger<ExceptionMiddleware> logger)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
            logger.LogError(ex, "An unhandled exception occurred while processing the request.");
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = Response<string>.Fail($"Internal Server Error. ErrorDetail:{exception.Message}",500);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsJsonAsync(response);
    }
}