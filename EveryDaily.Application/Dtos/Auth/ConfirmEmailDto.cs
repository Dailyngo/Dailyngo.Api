namespace EveryDaily.Application.Dtos.Auth;

public record ConfirmEmailDto
{
    public string ConfirmatinToken { get; init; }
    public DateTimeOffset EmailConfirmedDate { get; init; }
}