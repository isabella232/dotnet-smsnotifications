using System.ComponentModel.DataAnnotations;

namespace TVQMANotifications.Models.AccountViewModels {
    public class ExternalLoginViewModel{
        [Required] [EmailAddress] public string Email{ get; set; }
    }
}