using EveryDaily.Core.Entity;
using Microsoft.AspNetCore.Identity;

namespace EveryDaily.Domain.Entities;

public class RoleEntity : IdentityRole<Guid>, IEntityBase
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}