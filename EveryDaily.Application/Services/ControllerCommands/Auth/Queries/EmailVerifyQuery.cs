using EveryDaily.Core.Dtos;
using MediatR;

namespace EveryDaily.Application.Services.ControllerCommands.Auth.Queries;

public class EmailVerifyQuery : IRequest<Response<NoContent>>
{
    public string Email { get; set; }
    public string Token { get; set; }
}

public class EmailVerifyQueryHandler : IRequestHandler<EmailVerifyQuery, Response<NoContent>>
{
    public Task<Response<NoContent>> Handle(EmailVerifyQuery request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}