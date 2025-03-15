using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Core.Dtos;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Enums;
using EveryDaily.Domain.Prefix.Redis;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace EveryDaily.Application.Services.Jwt;

public class JwtTokenGenerator(
    IConfiguration configuration,
    IOptions<JwtSettings> jwtSettings,
    ILogger<JwtTokenGenerator> logger,
    ICacheService cacheService,
    UserManager<UserEntity> userManager)
{
    public async Task<string?> GenerateToken(UserEntity user)
    {
        try
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("nameid", user.Id.ToString()),
                new Claim("username", user.UserName ?? ""),
                new Claim("phone", string.Empty),
                new Claim("given_name", user.Name ?? ""),
                new Claim("surname", user.Surname ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var rawToken = new JwtSecurityToken(
                issuer: jwtSettings.Value.Issuer,
                audience: jwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(Convert.ToDouble(jwtSettings.Value.Ttl)),
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(rawToken);

            var prefix = RedisPrefix.GetAccessTokenKey(user.Id);
            await cacheService.SetAsync(prefix, token, TimeSpan.FromSeconds(jwtSettings.Value.Ttl));

            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Token oluştururken hata aldı" + ex.Message);
            throw;
        }
    }

    public async Task<string> GenerateRefreshToken(UserEntity user)
    {
        var token = "";
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var rawToken = new JwtSecurityToken(
                issuer: jwtSettings.Value.Issuer,
                audience: jwtSettings.Value.Audience,
                claims: new Claim[] { new(JwtRegisteredClaimNames.Sub, user.Id.ToString()) },
                expires: DateTime.UtcNow.AddSeconds(jwtSettings.Value.RefreshTtl),
                signingCredentials: signIn
            );
            token = new JwtSecurityTokenHandler().WriteToken(rawToken);
        }
        catch (Exception ex)
        {
            logger.LogError($"{nameof(GenerateRefreshToken)} throw an exception. Exception: {ex.Message}", ex);
        }

        var prefix = RedisPrefix.GetRefreshTokenKey(user.Id);
        await cacheService.SetAsync(prefix, token,
            TimeSpan.FromSeconds(jwtSettings.Value.RefreshTtl));
        return token;
    }

    public async Task<ValidateTokenResult> VerifyToken(string token, JwtTokenType tokenType)
    {
        if (string.IsNullOrEmpty(token))
            return new ValidateTokenResult(false, "Please provide a valid token!");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Value.Secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Value.Audience,
                ValidIssuer = jwtSettings.Value.Issuer,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true
            }, out _);
        }
        catch (SecurityTokenExpiredException)
        {
            return new ValidateTokenResult(false, "Token has expired! Please login to get a new token!");
        }
        catch (Exception ex)
        {
            logger.LogError($"Token validation failed. Exception: {ex.Message}", ex);
            return new ValidateTokenResult(false, "Token validation failed.");
        }

        var userId = GetClaim(token, JwtRegisteredClaimNames.NameId);
        if (string.IsNullOrEmpty(userId))
            return new ValidateTokenResult(false, "Invalid token! Please login to get a new token!");

        var userExistsInCache = await cacheService.ExistsAsync(RedisPrefix.IsExistUserKey(userId));

        if (!userExistsInCache)
        {
            var userExists = await userManager.Users.AnyAsync(a => a.Id == Guid.Parse(userId) & a.EmailConfirmed);

            if (!userExists)
                return new ValidateTokenResult(false, "User not found.");
            
            await cacheService.SetAsync(RedisPrefix.IsExistUserKey(userId), userId, TimeSpan.FromMinutes(30));
        }

        var prefix = RedisPrefix.GetAccessTokenKey(Guid.Parse(userId));
        var redisToken = await cacheService.GetAsync(prefix);
        if (redisToken != token)
            return new ValidateTokenResult(false, "Token does not match the one in Redis.");

        return new ValidateTokenResult(true, string.Empty, userId, "")
        {
            UserClaims = GetClaims(token)
        };
    }

    public string? GetClaim(string token, string claimType)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
            return string.Empty;

        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        var stringClaimValue = securityToken?.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value;
        return stringClaimValue;
    }

    private List<Claim> GetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
            return [];

        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        var stringClaimValue = securityToken?.Claims.ToList();
        return stringClaimValue ?? [];
    }
}