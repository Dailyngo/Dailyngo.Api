using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents;
using EveryDaily.Persistence.BaseRepositories;
using MediatR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Test.Commands;

public class TestCreateCommand : IRequest<Response<NoContent>>
{
    public TestModel TestModel { get; set; }
}

public class TestCreateCommandHandler (MongoDocContext context)
    : IRequestHandler<TestCreateCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(TestCreateCommand request, CancellationToken cancellationToken)
    {
        var test = request.TestModel;
        test.CreatedAt = DateTimeOffset.UtcNow;
        await context.TestModels.Collection.InsertOneAsync(test,null,cancellationToken);
        return Response<NoContent>.Success(200);
    }
}