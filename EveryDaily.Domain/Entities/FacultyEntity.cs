using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveryDaily.Core.Entity;
namespace EveryDaily.Domain.Entities
{
    public class FacultyEntity : EntityBase
    {
        public Guid UniversityId { get; set; }
        public string Name { get; set; }

        public UniversityEntity University { get; set; }

        public List<DepartmentEntity> Departments { get; set;}
    }
}
