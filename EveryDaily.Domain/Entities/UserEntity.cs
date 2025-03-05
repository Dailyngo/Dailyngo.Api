using EveryDaily.Core.Entity;
using Microsoft.AspNetCore.Identity;

namespace EveryDaily.Domain.Entities;

public class UserEntity : IdentityUser<Guid>, IEntityBase
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string FullName => $"{Name} {Surname}";
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}