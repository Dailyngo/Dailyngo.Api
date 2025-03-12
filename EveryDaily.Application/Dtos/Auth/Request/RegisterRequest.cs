using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace EveryDaily.Application.Dtos.Auth.Request;

public class RegisterRequest
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}