using EveryDaily.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Domain.Entities
{
    public class UniversityEntity : EntityBase
    {
        public string Name { get; set; }
        public string Adress { get; set; }

        public List<FacultyEntity> Faculties { get; set; }
    }
}
