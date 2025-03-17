using EveryDaily.Core.Entity;
using EveryDaily.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<FacultyEntity> Faculties { get; set; }
    public DbSet<UniversityEntity> Universities { get; set; }
    public DbSet<AboutEntity> Abouts { get; set; }
    public DbSet<DepartmentEntity> Departments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().ToTable("Users");
        modelBuilder.Entity<RoleEntity>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles").HasKey(p => new { p.UserId, p.RoleId });
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins").HasNoKey();
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens").HasNoKey();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => (x.Entity is IEntityBase) && (x.State == EntityState.Added 
                                                     | x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((IEntityBase)entity.Entity).CreatedAt = now;
            }
            if (entity.State == EntityState.Modified)
                ((IEntityBase)entity.Entity).UpdatedAt = now;
        }
    }
    
}