using System.ComponentModel.DataAnnotations;

namespace MemberShipSys.Models
{
    public class ResetPasswordViewModel
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }

        [Required(ErrorMessage = "密碼為必填")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "密碼與確認密碼不相符")]
        public required string ConfirmPassword { get; set; }
    }
}
