using EveryDaily.Application.Dtos.Statistic;
using EveryDaily.Application.Socket;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Statistic;

public class CurrentStatisticQuery : IRequest<Response<CurrentStatisticResponse>>
{
}

public class CurrentStatisticQueryHandler(AppDbContext appDbContext, MongoDocContext mongoDocContext)
    : IRequestHandler<CurrentStatisticQuery, Response<CurrentStatisticResponse>>
{
    public async Task<Response<CurrentStatisticResponse>> Handle(CurrentStatisticQuery request,
        CancellationToken cancellationToken)
    {
        var onlineUserCount = NotificationHub.OnlineUsers.Count;
        var totalUserCount =
            await appDbContext.Users.CountAsync(x => !x.IsDeleted, cancellationToken: cancellationToken);

        var filter = Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false);
        var totalPostCount =
            await mongoDocContext.Posts.Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        
        var response = new CurrentStatisticResponse
        {
            OnlineUserCount = onlineUserCount,
            TotalUserCount = totalUserCount,
            TotalPostCount = totalPostCount
        };

        return Response<CurrentStatisticResponse>.Success(response, 200);
    }
}