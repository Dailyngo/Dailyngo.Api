using EveryDaily.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Domain.Entities
{
    public class DepartmentEntity : EntityBase
    {
        public Guid FacultyId { get; set; }

        public string Name { get; set; }

        public FacultyEntity Faculty { get; set; }
        public List<AboutEntity> Abouts { get; set; }
        

    }
}
