using System.ComponentModel.DataAnnotations;

namespace Shopping.Models.ViewModel
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }

        [Required]
        [Display(Name = "Recovery Code")]
        public int RecoveryCode { get; set; }

        [Required]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}
