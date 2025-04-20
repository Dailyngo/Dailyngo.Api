using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Commands
{
    public class UpdatePasswordCommand : IRequest<Response<string>>
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }
    public class UpdatePasswordCommandHandler(
        AppDbContext dbContext,
        UserManager<UserEntity> userManager,
        IUserService userService)
        : IRequestHandler<UpdatePasswordCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
                return Response<string>.Fail(AuthErrorMessage.UserNotFound);

            if (request.NewPassword != request.NewPasswordConfirm)
                return Response<string>.Fail("Yeni şifreler uyuşmuyor.");

            var passwordCheck = await userManager.CheckPasswordAsync(user, request.OldPassword);

            if (!passwordCheck)
                return Response<string>.Fail("Eski şifre hatalı.");

            if (request.OldPassword == request.NewPassword)
                return Response<string>.Fail("Yeni şifre, eski şifreyle aynı olamaz.");


            var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
                return Response<string>.Fail($"Şifre güncellenirken bir hata oluştu.\n {string.Join(", ", result.Errors.Select(x => x.Description))}");

            return Response<string>.Success("Şifre başarıyla güncellendi.", 200);
        }
    }
}
