using EveryDaily.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EveryDaily.Persistence;

public static class Seed
{
    public static async Task SeedCollectorData(IServiceProvider serviceProvider)
    {
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
            if (result.Succeeded) Console.WriteLine("Admin user created.");
            else result.Errors.ToList().ForEach(error => Console.WriteLine(error.Description));
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
}