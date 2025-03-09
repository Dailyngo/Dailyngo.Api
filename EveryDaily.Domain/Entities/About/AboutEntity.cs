using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveryDaily.Core.Entity;

namespace EveryDaily.Domain.Entities.About
{
    public class AboutEntity : EntityBase
    {
        public Guid UserId { get; set; }
        public Guid DepartmentId { get; set; }
        public UserEntity User { get; set; }
        public DepartmentEntity Department { get; set; }
    }
}
