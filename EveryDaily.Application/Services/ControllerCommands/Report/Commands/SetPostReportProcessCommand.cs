using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Report.Commands;

public class SetPostReportProcessCommand : IRequest<Response<NoContent>>
{
    public string Id { get; set; }
}

public class SetPostReportProcessCommandHandler(MongoDocContext mongoDocContext) 
    : IRequestHandler<SetPostReportProcessCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(SetPostReportProcessCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<ReportDoc>.Filter.Eq(x => x.PostId, ObjectId.Parse(request.Id));
        var update = Builders<ReportDoc>.Update.Set(x => x.IsProcess, true);

        await mongoDocContext.Reports.Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return Response<NoContent>.Success(200);
    }
}