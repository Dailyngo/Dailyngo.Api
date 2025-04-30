using EveryDaily.Application.Dtos.Report.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EveryDaily.Application.Services.ControllerCommands.Report.Queries;

public class GetPostReportQuery : IRequest<Response<List<PostReportResponse>>>
{
}

public class GetPostReportQueryHandler(MongoDocContext mongoDocContext, AppDbContext appDbContext)
    : IRequestHandler<GetPostReportQuery, Response<List<PostReportResponse>>>
{
    public async Task<Response<List<PostReportResponse>>> Handle(GetPostReportQuery request,
        CancellationToken cancellationToken)
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

        var postFilter = Builders<PostDoc>.Filter.In(x => x.Id, postReports.Select(x => x.PostId));
        var posts = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .ToListAsync(cancellationToken: cancellationToken);

        var postReportResponses = posts.Select(x => new PostReportResponse()
        {
            Id = x.Id.ToString(),
            IsDeleted = x.IsDeleted,
            ReportDetails = postReports
                .Where(y => y.PostId == x.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReportDetailResponse()
                {
                    Id = r.Id.ToString(),
                    Reason = r.ReportReason,
                    IsProcess = r.IsProcess,
                    ReportedBy = users.FirstOrDefault(u => u.Id == Guid.Parse(r.UserId))
                                 ?? new IdNameResponse<Guid>()
                                 {
                                     Id = Guid.Empty,
                                     Name = "Bilinmeyen Kullanıcı"
                                 },
                    CreatedAt = r.CreatedAt.Value
                }).ToList()
        }).ToList();

        return Response<List<PostReportResponse>>.Success(postReportResponses, 200);
    }
}