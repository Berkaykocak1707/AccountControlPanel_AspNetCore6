using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AccountControlPanel_AspNetCore6.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email
        {
            get; set;
        }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password
        {
            get; set;
        }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword
        {
            get; set;
        }

        public string Token
        {
            get; set;
        }
    }

}
