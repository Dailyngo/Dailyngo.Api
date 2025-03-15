using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Queries;

public class EmailVerifyQuery : IRequest<Response<NoContent>>
{
    public string VerificationCode { get; set; }
}

public class EmailVerifyQueryHandler(AppDbContext appDbContext, ICacheService cacheService, IUserService userService)
    : IRequestHandler<EmailVerifyQuery, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(EmailVerifyQuery request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();
        var user = await appDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        
        if (user == null)
            return Response<NoContent>.Fail(AuthErrorMessage.UserNotFound);
        
        var verifyCode = await cacheService.GetAsync(RedisPrefix.GetEmailVerificationKey(userId));
        
        if(string.IsNullOrEmpty(verifyCode))
            return Response<NoContent>.Fail(AuthErrorMessage.EmailVerificationCodeNotFound); // todo duzeltilecek
        
        if (verifyCode != request.VerificationCode)
            return Response<NoContent>.Fail(AuthErrorMessage.EmailVerificationCodeNotMatch);

        user.EmailConfirmed = true;
        await appDbContext.SaveChangesAsync(cancellationToken);
        await cacheService.DeleteAsync(RedisPrefix.GetEmailVerificationKey(userId));
        
        return Response<NoContent>.Success(200);
    }
}