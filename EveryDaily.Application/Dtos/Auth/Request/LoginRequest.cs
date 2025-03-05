namespace EveryDaily.Application.Dtos.Auth.Request;

public class LoginRequest
{
    public string EmailOrUserName { get; set; }
    public string Password { get; set; }
}