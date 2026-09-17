namespace MemberShipSys.Models
{
    public class MemberListItemViewModel
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginCount { get; set; }
        public PasswordHashAlgorithm? CurrentHashAlgorithm { get; set; }
        public required IList<string> Roles { get; set; }
    }
}
