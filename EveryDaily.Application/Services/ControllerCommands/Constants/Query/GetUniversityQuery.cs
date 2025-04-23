using EveryDaily.Application.Dtos.Constants.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EveryDaily.Application.Services.ControllerCommands.Constants.Query
{
    public class GetUniversityQuery : IRequest<Response<List<GetDepartmentsResponse>>>
    {

    }

    public class GetUniversityQueryHandler(AppDbContext dbContext)
        : IRequestHandler<GetUniversityQuery, Response<List<GetDepartmentsResponse>>>
    {
        public async Task<Response<List<GetDepartmentsResponse>>> Handle(GetUniversityQuery request, CancellationToken cancellationToken)
        {
            var universities = await dbContext.Universities
                .Select(u => new GetDepartmentsResponse
                {
                    Id = u.Id,
                    Name = u.Name
                })
                .ToListAsync(cancellationToken);

            return Response<List<GetDepartmentsResponse>>.Success(universities, 200);
        }
    }
}
