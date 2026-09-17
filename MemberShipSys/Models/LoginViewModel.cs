using System.ComponentModel.DataAnnotations;

namespace MemberShipSys.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email為必填")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "密碼為必填")]
        [DataType(dataType: DataType.Password)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
