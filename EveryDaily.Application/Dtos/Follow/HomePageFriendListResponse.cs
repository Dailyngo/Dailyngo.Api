using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.Follow
{
    public class HomePageFriendListResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
