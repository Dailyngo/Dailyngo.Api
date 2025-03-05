using System.Security.Claims;

namespace EveryDaily.Core.Dtos;

public record ValidateTokenResult(
    bool IsValid,
    string Message,
    string? Token = null,
    string? RefreshToken = null,
    List<Claim>? UserClaims = null);