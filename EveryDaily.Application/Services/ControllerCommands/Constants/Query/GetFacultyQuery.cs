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
    public class GetFacultyQuery : IRequest<Response<List<GetDepartmentsResponse>>>
    {
       public Guid UniversityId { get; set; }
    }

    public class GetFacultyQueryHandler(AppDbContext dbContext)
        : IRequestHandler<GetFacultyQuery, Response<List<GetDepartmentsResponse>>>
    {
        public async Task<Response<List<GetDepartmentsResponse>>> Handle(GetFacultyQuery request, CancellationToken cancellationToken)
        {
            var faculties = await dbContext.Faculties
                .Where(f => f.UniversityId == request.UniversityId)
                .Select(f => new GetDepartmentsResponse
                {
                    Id = f.Id,
                    Name = f.Name
                })
                .ToListAsync(cancellationToken);

            return Response<List<GetDepartmentsResponse>>.Success(faculties, 200);
        }
    }
}
