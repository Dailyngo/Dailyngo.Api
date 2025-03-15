using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Application.Services.UserService;
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


    public class UpdateAboutHandler(AppDbContext appDbContext,IUserService userService)
        : IRequestHandler<UpdateAboutCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(UpdateAboutCommand request, CancellationToken cancellationToken)
        {


            var userID = userService.GetUserId();
            var email = userService.GetUserEmail();


            var about = await appDbContext.Abouts.FirstOrDefaultAsync(x => x.Id == request.Data.Id && x.UserId == userID, cancellationToken);

            //var about = await appDbContext.Abouts
            //    .Include(i=> i.User)
            //    .FirstOrDefaultAsync(x => x.UserId == userID, cancellationToken);


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
