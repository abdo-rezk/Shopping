using System.ComponentModel.DataAnnotations;

namespace Shopping.Models.ViewModel
{
    public class RecoveryPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
