using System.ComponentModel.DataAnnotations;

namespace MemberShipSys.Models
{
    public class ResendConfirmationViewModel
    {
        [Required(ErrorMessage = "Email為必填")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        public required string Email { get; set; }
    }
}
