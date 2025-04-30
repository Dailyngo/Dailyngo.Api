using EveryDaily.Application.Dtos.Report.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Report.Queries;

public class GetPostReportQuery : IRequest<Response<List<PostReportResponse>>>
{
}

public class GetPostReportQueryHandler(MongoDocContext mongoDocContext,AppDbContext appDbContext) 
    : IRequestHandler<GetPostReportQuery, Response<List<PostReportResponse>>>
{
    public async Task<Response<List<PostReportResponse>>> Handle(GetPostReportQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<ReportDoc>.Filter.Where(x => x.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30));

        var postReports = await mongoDocContext.Reports.Collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken: cancellationToken);
        
        var userIds = postReports.Select(y => Guid.Parse(y.UserId)).ToList();
        var users = appDbContext.Users
            .Where(x => userIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new IdNameResponse<Guid>()
            {
                Id = x.Id,
                Name = x.FullName
            })
            .ToList();
        
        var postReportResponses = postReports.Select(x => new PostReportResponse
        {
            Id = x.Id.ToString(),
            PostId = x.PostId.ToString(),
            Reason = x.ReportReason,
            IsProcess = x.IsProcess,
            ReportedBy = users.FirstOrDefault(u => u.Id == Guid.Parse(x.UserId)) 
                ?? new IdNameResponse<Guid>()
                {
                    Id = Guid.Empty,
                    Name = "Bilinmeyen Kullanıcı"
                },
            CreatedAt = x.CreatedAt.Value
        }).ToList();
        
        return Response<List<PostReportResponse>>.Success(postReportResponses, 200);
    }
}