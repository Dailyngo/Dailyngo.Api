using EveryDaily.Domain.Enums;

namespace EveryDaily.Application.Dtos.About.Request
{
    public class UpdateAboutRequest
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public Gender Gender { get; set; }

    }
}
