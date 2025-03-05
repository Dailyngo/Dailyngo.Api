using EveryDaily.Application.Repositories;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence.BaseRepositories;
using MediatR;
using MongoDB.Bson;

namespace EveryDaily.Application.Services.ControllerCommands.Test.Commands;

public class TestCreateCommand : IRequest<Response<NoContent>>
{
    public TestModel TestModel { get; set; }
}

public class TestCreateCommandHandler (MongoDbRepository<TestModel,ObjectId> testRepository)
    : IRequestHandler<TestCreateCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(TestCreateCommand request, CancellationToken cancellationToken)
    {
        var test = request.TestModel;
        await testRepository.InsertAsync(test);
        return Response<NoContent>.Success(200);
    }
}