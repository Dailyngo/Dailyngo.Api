using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.About.Commands
{
    public class CreateAboutCommand : IRequest<Response<NoContent>>
    {
        public CreateAboutRequest Data { get; set; }
    }
    public class CreateAboutCommadHandler(AppDbContext appDbContext, IUserService userService)
        : IRequestHandler<CreateAboutCommand, Response<NoContent>>
    {

        
        public async Task<Response<NoContent>> Handle(CreateAboutCommand request, CancellationToken cancellationToken)
        {
            var departmentExist = await appDbContext.Departments.AnyAsync(x => x.Id == request.Data.DepartmentId, cancellationToken);

            if (!departmentExist)
            {
                return Response<NoContent>.Fail("Departman Bulunamadı", 404);
            }

            // var userId = await appDbContext.Users.Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);


            var userID = userService.GetUserId();
            var email = userService.GetUserEmail();

            var aboutEntity = new AboutEntity
            {
                UserId = userID,
                DepartmentId = request.Data.DepartmentId,
                BirthDate = request.Data.BirthDate,
                Gender = request.Data.Gender,
            };


            await appDbContext.AddAsync(aboutEntity,cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);


            return Response<NoContent>.Success(200);
        }
    }

}
