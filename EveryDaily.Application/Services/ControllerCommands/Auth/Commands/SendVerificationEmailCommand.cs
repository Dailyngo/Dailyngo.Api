using System.Text.Json;
using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Auth;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Commands;

public class SendVerificationEmailCommand : IRequest<Core.Dtos.Response<EmailVerificationResponse>>;

public class SendVerificationEmailCommandHandler(
    AppDbContext appDbContext,
    IBusControl busControl,
    IUserService userService,
    UserManager<UserEntity> userManager,
    ICacheService cacheService)
    : IRequestHandler<SendVerificationEmailCommand, Core.Dtos.Response<EmailVerificationResponse>>
{
    public async Task<Core.Dtos.Response<EmailVerificationResponse>> Handle(SendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();
        var user = await appDbContext.Users.FirstOrDefaultAsync(
            x => x.Id == userId && !x.IsDeleted, cancellationToken);
        
        if (!await cacheService.ExistsAsync(RedisPrefix.GetEmailVerificationKey(user.Id)))
        {
            var confirmationToken =
                await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            var confirmation = new ConfirmEmailDto
            {
                ConfirmatinToken = confirmationToken,
                EmailConfirmedDate = DateTimeOffset.UtcNow.AddMinutes(3)
            };
            
            await cacheService.SetAsync(RedisPrefix.GetEmailVerificationKey(user.Id),
                JsonSerializer.Serialize(confirmation), TimeSpan.FromMinutes(3));

            await busControl.Publish(new EmailSendingMessage()
            {
                To = user.Email,
                Subject = "Dailyngo - Email Confirmation",
                Body = $"Please confirm your email verification code: [{confirmationToken}]"
            }, cancellationToken);

            return Core.Dtos.Response<EmailVerificationResponse>.Success(new EmailVerificationResponse()
            {
                EmailConfirmedDate = confirmation.EmailConfirmedDate
            },201);
        }

        var confirmStr = await cacheService.GetAsync(RedisPrefix.GetEmailVerificationKey(user.Id));

        var confirmObj = JsonSerializer.Deserialize<ConfirmEmailDto>(confirmStr);
        
        return Core.Dtos.Response<EmailVerificationResponse>.Success(new EmailVerificationResponse()
        {
            EmailConfirmedDate = confirmObj.EmailConfirmedDate
        },200);
    }
}