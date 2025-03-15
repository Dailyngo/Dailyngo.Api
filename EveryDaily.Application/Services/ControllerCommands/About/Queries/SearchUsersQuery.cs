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
    public class SearchUsersQuery : IRequest<Response<List<SearchUserResponse>>>
    {
        public string Username { get; set; }
        public SearchUsersQuery(string username)
        {
            Username = username;
        }

        public class SearchUsersQueryHandler(AppDbContext appDbContext)
                 : IRequestHandler<SearchUsersQuery, Response<List<SearchUserResponse>>>
        {
            public async Task<Response<List<SearchUserResponse>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                    return Response<List<SearchUserResponse>>.Fail("Kullanıcı adı boş olamaz", 404);

                var response = await appDbContext.Users
                   .Where(x => x.Name.Contains(request.Username) ||
                        x.Surname.Contains(request.Username) ||
                        x.UserName.Contains(request.Username))
                    .Select(u => new SearchUserResponse
                    {
                      //   Id = u.Id,
                         Username = u.UserName,
                         Name = u.Name,
                         Surname = u.Surname,
                
                    })
                       .ToListAsync(cancellationToken);

                if (response.Count == 0)
                    return Response<List<SearchUserResponse>>.Fail("Kullanıcı bulunamadı", 404);

                return Response<List<SearchUserResponse>>.Success(response);
            }
        }
    }
}
