using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents;
using EveryDaily.Persistence.BaseRepositories;
using MediatR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Test.Queries;

public class TestGetAllQuery : IRequest<Response<List<TestModel>>>
{
}

public class TestGetAllQueryHandler(MongoDocContext context)
    : IRequestHandler<TestGetAllQuery, Response<List<TestModel>>>
{
    public async Task<Response<List<TestModel>>> Handle(TestGetAllQuery request, CancellationToken cancellationToken)
    {
        var result = await context.TestModels.Collection.Find(r => true).ToListAsync(cancellationToken);
        return Response<List<TestModel>>.Success(result.ToList());
    }
}