using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.Follow
{
    public class UserFollowResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
    }
}
