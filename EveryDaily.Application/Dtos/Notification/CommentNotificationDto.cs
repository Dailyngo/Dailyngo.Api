using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.Notification
{
    public class CommentNotificationDto
    {
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string? CommentText { get; set; } 
        public string RelatedEntityId { get; set; } 
    }
}
