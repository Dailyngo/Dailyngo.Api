using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Enums;

namespace EveryDaily.Application.Dtos.About.Request
{
    public class CreateAboutRequest
    {
        public Guid DepartmentId { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public Gender Gender { get; set; }
    }
}
