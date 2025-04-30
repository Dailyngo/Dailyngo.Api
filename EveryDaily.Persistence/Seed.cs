using System.Reflection;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EveryDaily.Persistence;

public static class Seed
{
    public static async Task SeedCollectorData(IServiceProvider serviceProvider)
    {
        await SeedRoles(serviceProvider);
        await SeedUserData(serviceProvider);
        await SeedEducationData(serviceProvider);
    }

    private static async Task SeedUserData(IServiceProvider serviceProvider)
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
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Permission.SuperAdmin.ToString());
            }
        }
        else
        {
            var user = await userManager.FindByNameAsync("admin");
            if (user == null) return;
            
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(Permission.SuperAdmin.ToString()))
            {
                await userManager.AddToRoleAsync(user, Permission.SuperAdmin.ToString());
            }
        }
    }

    private static async Task SeedEducationData(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existDepartment = await appDbContext.Departments.AnyAsync();
        
        if (existDepartment) return;
        
        var university = new UniversityEntity
        {
            Name = "Erzurum Teknik Üniversitesi",
            Adress = "Yakutiye/Erzurum",
        };

        await appDbContext.Universities.AddAsync(university);
        
        var faculties = new List<FacultyEntity>
        {
            new ()
            {
                Name = "Mühendislik Fakültesi",
                UniversityId = university.Id
            },
            new ()
            {
                Name = "İktisadi ve İdari Bilimler Fakültesi",
                UniversityId = university.Id
            }
        };
        
        await appDbContext.Faculties.AddRangeAsync(faculties);
        
        var departments = new List<DepartmentEntity>
        {
            new ()
            {
                Name = "Bilgisayar Mühendisliği",
                FacultyId = faculties[0].Id
            },
            new ()
            {
                Name = "İnşaat Mühendisliği",
                FacultyId = faculties[0].Id
            },
            new ()
            {
                Name = "İşletme",
                FacultyId = faculties[1].Id
            },
            new ()
            {
                Name = "İktisat",
                FacultyId = faculties[1].Id
            }
        };
        
        await appDbContext.Departments.AddRangeAsync(departments);
        await appDbContext.SaveChangesAsync();
    }
    
    private static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleEntity>>();

        var defaultRoles = Enum.GetValues(typeof(Permission)).Cast<Permission>().Select(x => x.ToString()).ToList();
        
        var existRoles = await roleManager.Roles.Select(x => x.Name).ToListAsync();
        
        var rolesToAdd = defaultRoles.Where(x => !existRoles.Contains(x)).ToList();
        
        if (rolesToAdd.Count == 0) return;
        
        foreach (var role in rolesToAdd)
        {
            await roleManager.CreateAsync(new()
            {
                CreatedAt = DateTimeOffset.UtcNow,
                Name = role,
                NormalizedName = role.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
        }
    }
}