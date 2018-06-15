using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVQMANotifications.Models
{
    public class DashboardModel
    {
        public int SubscriberCount { get; set; }
        public string Message { get; set; }
        public List<Message> LastMessages { get; set; }
    }
}
