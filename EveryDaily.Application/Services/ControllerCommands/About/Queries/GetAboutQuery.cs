using EveryDaily.Application.Dtos.About.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Services.ControllerCommands.About.Queries
{
    public class GetAboutQuery : IRequest<Response<GetAboutResponse>>
    {

    }

    public class GetAboutQueryHandler(AppDbContext appDbContext)
        : IRequestHandler<GetAboutQuery, Response<GetAboutResponse>>
{
        public async Task<Response<GetAboutResponse>> Handle(GetAboutQuery request, CancellationToken cancellationToken)
        {

            var userId = await appDbContext.Users.Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

            var about = await appDbContext.Abouts
                .Include(i => i.Department)
                .ThenInclude(i => i.Faculty)
                .ThenInclude(i => i.University)
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (about == null)
            {
                return Response<GetAboutResponse>.Fail("Hakkında Bulunamadı", 404);
            }

            var response = new GetAboutResponse
            {
                BirthDate = about.BirthDate,
                Gender = about.Gender,
                Department = new GetAboutDepartmentResponse()
                {
                    Id = about.DepartmentId,
                    Name = about.Department.Name,
                    Faculty = new GetAboutFacultyResponse()
                    {
                        Id = about.Department.FacultyId,
                        Name = about.Department.Faculty.Name,
                        University = new GetAboutUniversityResponse()
                        {
                            Id = about.Department.Faculty.UniversityId,
                            Name = about.Department.Faculty.University.Name,
                        }
                    }
                }
            };
            return Response<GetAboutResponse>.Success(response);
        }
    }
}
