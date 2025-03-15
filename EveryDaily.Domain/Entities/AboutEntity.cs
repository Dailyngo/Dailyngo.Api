using EveryDaily.Core.Entity;
using EveryDaily.Domain.Enums;

namespace EveryDaily.Domain.Entities
{
    public class AboutEntity : EntityBase
    {
        public Guid UserId { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public UserEntity User { get; set; }
        public DepartmentEntity Department { get; set; }
    }
}
