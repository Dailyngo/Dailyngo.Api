using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.Notification
{
    public class NotificationResponse
    {
        public List<FollowNotificationDto>? FollowRequests { get; set; }
    }
}
