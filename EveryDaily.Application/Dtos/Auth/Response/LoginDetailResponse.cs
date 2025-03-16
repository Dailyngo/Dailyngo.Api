namespace EveryDaily.Application.Dtos.Auth.Response;

public class LoginDetailResponse
{
    public bool IsRegistered { get; set; }
    public bool IsEmailConfirmed { get; set; }
}

public class EmailVerificationResponse
{
    public DateTimeOffset EmailConfirmedDate { get; set; }
}