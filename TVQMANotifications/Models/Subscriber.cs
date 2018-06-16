using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVQMANotifications.Models {
    public class Subscriber{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriberId{ get; set; }
        public string Number{ get; set; }

        public void SendSms() { }
    }
}