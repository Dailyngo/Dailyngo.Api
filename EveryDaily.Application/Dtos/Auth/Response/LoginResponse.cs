using System.Text.Json.Serialization;

namespace EveryDaily.Application.Dtos.Auth.Response;

public class LoginResponse
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ErrorMessage { get; set; }
}