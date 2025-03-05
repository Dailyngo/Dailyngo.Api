using System.ComponentModel.DataAnnotations;
using EveryDaily.Core.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Core.ControllerBases;

public class CustomControllerBase : ControllerBase
{
    public IActionResult CreateActionResultInstance<T>(Response<T> response)
    {
        if (response.StatusCode == 204)
            return NoContent();
        return new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };
    }

    // ///<summary>
    // ///<para>Return an action result for Fluent Validation's ValidationResult object.</para>
    // ///</summary>
    // public IActionResult CreateActionResultInstance<T>(ValidationResult validationResult)
    // {
    //     return new ObjectResult(Response<T>.Fail(validationResult, 400))
    //     {
    //         StatusCode = 400
    //     };
    // }
    //public IActionResult CreateActionResultInstance<T>(ValidationResult validationResult)
    //{
    //    return new ObjectResult(Response<T>.Fail(validationResult))
    //    {
    //        StatusCode = 400
    //    };
    //}

    [ApiExplorerSettings(IgnoreApi = true)]
    public string? GetUserId()
    {
        if (HttpContext.Items.Any(a => (string)a.Key == "UserId"))
            return HttpContext.Items.FirstOrDefault(a => (string)a.Key == "UserId").Value?.ToString();
        else return null;
    }
}