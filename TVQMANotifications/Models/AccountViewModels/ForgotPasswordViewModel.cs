using System.ComponentModel.DataAnnotations;

namespace TVQMANotifications.Models.AccountViewModels {
    public class ForgotPasswordViewModel{
        [Required] [EmailAddress] public string Email{ get; set; }
    }
}