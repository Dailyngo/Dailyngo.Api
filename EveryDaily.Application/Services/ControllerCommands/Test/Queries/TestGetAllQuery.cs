using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence.BaseRepositories;
using MediatR;
using MongoDB.Bson;

namespace EveryDaily.Application.Services.ControllerCommands.Test.Queries;

public class TestGetAllQuery : IRequest<Response<List<TestModel>>>
{
}

public class TestGetAllQueryHandler(MongoDbRepository<TestModel, ObjectId> testRepository, IUserService userService)
    : IRequestHandler<TestGetAllQuery, Response<List<TestModel>>>
{
    public async Task<Response<List<TestModel>>> Handle(TestGetAllQuery request, CancellationToken cancellationToken)
    {
        var result = await testRepository.GetAllAsync();

// ornek kullanim
        var userID = userService.GetUserId();
        var email = userService.GetUserEmail();

        return Response<List<TestModel>>.Success(result.ToList());
    }
}