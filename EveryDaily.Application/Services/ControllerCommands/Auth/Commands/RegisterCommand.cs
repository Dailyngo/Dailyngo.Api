using System.ComponentModel.DataAnnotations;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Commands;

public class RegisterCommand : IRequest<Response<NoContent>>
{
    public RegisterRequest Data { get; set; }
}

public class RegisterCommandHandler(AppDbContext appDbContext, UserManager<UserEntity> userManager)
    : IRequestHandler<RegisterCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (!request.Data.Email.EndsWith("edu.tr"))
            return Response<NoContent>.Fail(AuthErrorMessage.InvalidEmailAddress);

        if (!IsMailFormatValid(request.Data.Email))
            return Response<NoContent>.Fail(AuthErrorMessage.InvalidEmailFormat);

        if (!request.Data.Password.Equals(request.Data.ConfirmPassword))
            return Response<NoContent>.Fail(AuthErrorMessage.PasswordsDoNotMatch);

        var user = new UserEntity
        {
            Email = request.Data.Email,
            UserName = request.Data.UserName,
            Name = request.Data.Name,
            Surname = request.Data.Surname
        };

        var userNameExists = await appDbContext.Users
            .AnyAsync(x => x.UserName.ToLower().Equals(user.UserName.ToLower()), cancellationToken);

        if (userNameExists)
            return Response<NoContent>.Fail(AuthErrorMessage.UserNameAlreadyExists);

        var result = await userManager.CreateAsync(user, request.Data.Password);

        return !result.Succeeded
            ? Response<NoContent>.Fail(string.Join("\n", result.Errors.Select(x => x.Description)))
            : Response<NoContent>.Success(200);
    }

    private bool IsMailFormatValid(string email) => new EmailAddressAttribute().IsValid(email);
}