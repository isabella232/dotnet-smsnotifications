using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVQMANotifications.Models {
    public class SendLog{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SendLogId{ get; set; }

        public int SubscriberId{ get; set; }
        public int MessageId{ get; set; }
        public string SinchMessageId{ get; set; }
        public DateTime DateSent{ get; set; }
        public DateTime DateDelivered{ get; set; }
        public string Status{ get; set; }
        public virtual Subscriber Subscriber{ get; set; }
    }
}