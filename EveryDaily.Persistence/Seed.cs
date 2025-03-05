using EveryDaily.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EveryDaily.Persistence;

public static class Seed
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {

        var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

        if (await userManager.FindByNameAsync("admin") == null)
        {
            var user = new UserEntity
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@dailygno.com",
                NormalizedEmail = "ADMIN@DAIYLNGO.COM",
                EmailConfirmed = true,
                PhoneNumber = "5555555555",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                Name = "Dailyngo",
                Surname = "Admin"
            };

            var result = await userManager.CreateAsync(user, "P@ssw0rd");
            if (result.Succeeded) Console.WriteLine("Admin user created.");
            else result.Errors.ToList().ForEach(error => Console.WriteLine(error.Description));
        }
    }
}