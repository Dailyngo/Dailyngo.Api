using EveryDaily.Application.Dtos.Constants.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EveryDaily.Application.Services.ControllerCommands.Constants.Querie
{
    public class GetDepartmentQuery : IRequest<Response<List<GetDepartmentsResponse>>>
    {
        public Guid FacultyId { get; set; }
    }

    public class GetDepartmentQueryHandler(AppDbContext dbContext) : IRequestHandler<GetDepartmentQuery, Response<List<GetDepartmentsResponse>>>
    {
        public async Task<Response<List<GetDepartmentsResponse>>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
        {
            var departments = await dbContext.Departments
                .Where(d => d.FacultyId == request.FacultyId)
                .Select(d => new GetDepartmentsResponse
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToListAsync(cancellationToken);

            return Response<List<GetDepartmentsResponse>>.Success(departments, 200);
        }
    }
}
