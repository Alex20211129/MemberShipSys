using System.ComponentModel.DataAnnotations;

namespace MemberShipSys.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Email為必填")]
        [EmailAddress(ErrorMessage ="Email格式不正確")]
        public required string Email { get; set; }
        [Required(ErrorMessage ="密碼為必填")]
        [DataType(dataType: DataType.Password)]
        public required string Password { get; set; } 
        [Required]
        [DataType(dataType: DataType.Password)]
        [Compare(nameof(Password),ErrorMessage ="密碼與確認密碼不相符")]
        public required string ConfirmPassword { get; set; }
    }
}
