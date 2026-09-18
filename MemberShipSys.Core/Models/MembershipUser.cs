using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace MemberShipSys.Models
{
    public class MembershipUser : IdentityUser
    {
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public bool IsDisabled { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginCount { get; set; }

        public PasswordHashAlgorithm? CurrentHashAlgorithm { get; set; }
    }
}
