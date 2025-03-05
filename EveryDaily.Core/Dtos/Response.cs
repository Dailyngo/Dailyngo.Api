using System.Text.Json.Serialization;
using EveryDaily.Core.Pagination;

namespace EveryDaily.Core.Dtos;

public class Response<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    [JsonIgnore] public int StatusCode { get; set; }

    [JsonIgnore] public bool IsSuccessful { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? messages { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Pager? Pagination { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Errors { get; set; }


    // Static Factory Method
    public static Response<T> Success(T? data, int statusCode = 200, Pager? pager = null)
    {
        return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessful = true, Pagination = pager };
    }

    public static Response<T> Success(int statusCode)
    {
        return new Response<T> { Data = default(T), StatusCode = statusCode, IsSuccessful = true };
    }

    public static Response<T> Fail(List<string> errors, int statusCode)
    {
        return new Response<T>
        {
            messages = errors.FirstOrDefault(),
            StatusCode = statusCode,
            IsSuccessful = false
        };
    }
    public static Response<T> NotFound(List<string> errors, int statusCode)
    {
        return new Response<T>
        {
            messages = errors.FirstOrDefault(),
            StatusCode = statusCode,
            IsSuccessful = false
        };
    }

    public static Response<T> Fail(string error, int statusCode = 400)
    {
        return new Response<T> { messages = error, StatusCode = statusCode, IsSuccessful = false };
    }
    public static Response<T> NotFound(string error = "not.found", int statusCode = 404)
    {
        return new Response<T> { messages = error, StatusCode = statusCode, IsSuccessful = false };
    }
}