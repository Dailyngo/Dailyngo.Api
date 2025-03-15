using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.User.Response
{
    public class SearchUserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }

        public string FullName { get; set; }
    }
}