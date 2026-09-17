using System.ComponentModel.DataAnnotations;

namespace MemberShipSys.Models
{
    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "密碼為必填")]
        [DataType(dataType: DataType.Password)]
        public required string Password { get; set; }
        [Required]
        [DataType(dataType: DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "密碼與確認密碼不相符")]
        public required string ConfirmPassword { get; set; }

    }
}
