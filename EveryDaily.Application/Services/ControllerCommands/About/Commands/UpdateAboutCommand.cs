using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.About.Commands
{
    public class UpdateAboutCommand : IRequest<Response<NoContent>>
    {
        public UpdateAboutRequest Data { get; set; }
    }


    public class UpdateAboutHandler(AppDbContext appDbContext)
        : IRequestHandler<UpdateAboutCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(UpdateAboutCommand request, CancellationToken cancellationToken)
        {

            var userId = await appDbContext.Users.Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

            var about = await appDbContext.Abouts.FirstOrDefaultAsync(x => x.Id == request.Data.Id && x.UserId == userId, cancellationToken);

            if (about == null)
            {
                return Response<NoContent>.Fail("Hakkında Bulunamadı", 404);
            }

            var departmentExist = await appDbContext.Departments.AnyAsync(x => x.Id == request.Data.DepartmentId, cancellationToken);

            if (!departmentExist)
            {
                return Response<NoContent>.Fail("Departman Bulunamadı", 404);
            }

            about.BirthDate = request.Data.BirthDate;
            about.Gender = request.Data.Gender;
            about.DepartmentId = request.Data.DepartmentId;

            appDbContext.Abouts.Update(about);
            await appDbContext.SaveChangesAsync(cancellationToken);

            return Response<NoContent>.Success(200);
        }
    }
}
