using System.ComponentModel.DataAnnotations;

namespace TVQMANotifications.Models {
    public class SinchConfig
    {
        [Key]
        public string Key{ get; set; }
        public string Secret { get; set; }
        public string SinchNumber { get; set; }
        public string MarketingFooter { get; set; }

    }
}
