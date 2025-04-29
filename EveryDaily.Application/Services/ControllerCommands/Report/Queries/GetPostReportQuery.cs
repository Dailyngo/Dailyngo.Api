using EveryDaily.Application.Dtos.Report.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Report.Queries;

public class GetPostReportQuery : IRequest<Response<List<PostReportResponse>>>
{
    public bool IsProcess { get; set; }
}

public class GetPostReportQueryHandler(MongoDocContext mongoDocContext) 
    : IRequestHandler<GetPostReportQuery, Response<List<PostReportResponse>>>
{
    public async Task<Response<List<PostReportResponse>>> Handle(GetPostReportQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<ReportDoc>.Filter.Eq(x => x.IsProcess, request.IsProcess);

        var postReports = await mongoDocContext.Reports.Collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken: cancellationToken);
        
        var postReportResponses = postReports.Select(x => new PostReportResponse
        {
            Id = x.Id.ToString(),
            PostId = x.PostId.ToString(),
            Reason = x.ReportReason,
            IsProcess = x.IsProcess
        }).ToList();
        
        return Response<List<PostReportResponse>>.Success(postReportResponses, 200);
    }
}