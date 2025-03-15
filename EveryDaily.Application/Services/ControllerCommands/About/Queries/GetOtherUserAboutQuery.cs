using EveryDaily.Application.Dtos.About.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.About.Queries
{
    public class GetOtherUserAboutQuery : IRequest<Response<GetAboutResponse>>
    {
        public Guid UserID { get; set; } // Root’tan gelen kullanıcı ID'si

        public GetOtherUserAboutQuery(Guid userId)
        {
            UserID = userId;
        }
    }


    public class GetOtherUserAboutQueryHandler(AppDbContext appDbContext)
        : IRequestHandler<GetOtherUserAboutQuery, Response<GetAboutResponse>>
    {
        public  async Task<Response<GetAboutResponse>> Handle(GetOtherUserAboutQuery request, CancellationToken cancellationToken)
        {
            var about = await appDbContext.Abouts
                           .Include(i => i.Department)
                           .ThenInclude(i => i.Faculty)
                           .ThenInclude(i => i.University)
                           .FirstOrDefaultAsync(x => x.UserId == request.UserID, cancellationToken);

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
