using EveryDaily.Application.Dtos.About.Response;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Services.ControllerCommands.About.Queries
{
    public class SearchUsersQuery: IRequest<Response<List<SearchUserResponse>>>
    {
        public string SearchTerm { get; set; }
    }

    public class SearchUsersQueryHandler(AppDbContext appDbContext)
                : IRequestHandler<SearchUsersQuery, Response<List<SearchUserResponse>>>
    {
        public async Task<Response<List<SearchUserResponse>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var searchTerm = request.SearchTerm.ToLower();

            var response = await appDbContext.Users
               .Where(x => x.Name.ToLower().Contains(searchTerm) ||
                    x.Surname.ToLower().Contains(searchTerm) ||
                    x.UserName.ToLower().Contains(searchTerm))
                .Select(u => new SearchUserResponse
                {
                    Id = u.Id, 
                    Username = u.UserName,
                    FullName = u.FullName 

                })
                   .ToListAsync(cancellationToken);

            return Response<List<SearchUserResponse>>.Success(response);
        }
    }
}
