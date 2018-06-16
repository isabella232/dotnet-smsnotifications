using System.ComponentModel.DataAnnotations;

namespace TVQMANotifications.Models.AccountViewModels {
    public class LoginWithRecoveryCodeViewModel{
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode{ get; set; }
    }
}