using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.User.Response
{
    public class GetBirthdayListResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
     // public string UserName { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
    }
}
