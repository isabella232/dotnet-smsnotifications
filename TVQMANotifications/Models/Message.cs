using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVQMANotifications.Models {
    public class Message{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId{ get; set; }

        public String MessageContent{ get; set; }
        public DateTime DateSent{ get; set; }
        public virtual IEnumerable<SendLog> Logs{ get; set; }
    }
}