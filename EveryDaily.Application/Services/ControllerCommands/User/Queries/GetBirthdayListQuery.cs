using EveryDaily.Application.Dtos.User.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Services.ControllerCommands.User.Queries
{
    public class GetBirthdayListQuery : IRequest<Response<List<GetBirthdayListResponse>>>
    {
    }

        public class GetBirthdayPeopleQueryHandler(AppDbContext appDbContext)
        : IRequestHandler<GetBirthdayListQuery, Response<List<GetBirthdayListResponse>>>
        {
            public async Task<Response<List<GetBirthdayListResponse>>> Handle(GetBirthdayListQuery request, CancellationToken cancellationToken)
            {
                var today = DateTimeOffset.UtcNow;

                var response = await appDbContext.Abouts
                    .Include(a => a.User)
                    .Where(a => a.BirthDate.HasValue &&
                                a.BirthDate.Value.Day == today.Day &&
                                a.BirthDate.Value.Month == today.Month)
                    .Select(a => new GetBirthdayListResponse
                    {
                        Id = a.UserId,
                        FullName = a.User.FullName,
                        // UserName = a.User.UserName,
                        BirthDate = a.BirthDate
                    })
                    .ToListAsync(cancellationToken);
            if (response.Count == 0)
            {
                // Bugün doğum günü olan kimse yoksa bu mesajı döndür
                return Response<List<GetBirthdayListResponse>>.Success(
                    new List<GetBirthdayListResponse> { new GetBirthdayListResponse { FullName = "Bugün kimsenin doğum günü yok." } }
                );
            }

            return Response<List<GetBirthdayListResponse>>.Success(response);
            }
        }

    
}