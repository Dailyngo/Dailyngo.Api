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
        public string UserName { get; set; }
        public bool IsFollower { get; set; } // Kullanıcıyı takip eden kişi mi?
        public bool IsFollowing { get; set; } // Kullanıcıyı takip ediyor mu?
    }
}
