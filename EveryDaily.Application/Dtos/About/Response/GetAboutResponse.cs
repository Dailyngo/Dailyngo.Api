using EveryDaily.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.About.Response
{
    public class GetAboutResponse
    {
        public GetAboutDepartmentResponse Department { get; set; }

        public DateTimeOffset? BirthDate { get; set; }
        public Gender Gender { get; set; }
    }

    public class GetAboutDepartmentResponse
    {
        public Guid Id { get; set; }
        public GetAboutFacultyResponse Faculty { get; set; }
        public string Name { get; set; }
    }

    public class GetAboutFacultyResponse
    {
        public Guid Id { get; set; }
        public GetAboutUniversityResponse University { get; set; }
        public string Name { get; set; }
    }
    public class GetAboutUniversityResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }





}
