using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Notification
{
    public class NotificationReponse
    {
        public string userId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public bool isRead { get; set; }
    }
}
