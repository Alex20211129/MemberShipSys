using MemberShipSys.Models;
using Microsoft.AspNetCore.Identity;

namespace MemberShipSys.Services.Hashing
{
    public class MembershipPasswordHasher : IPasswordHasher<MembershipUser>
    {
        private readonly Dictionary<PasswordHashAlgorithm, IPasswordHashStrategy> _strategies;
        private readonly ICurrentAlgorithmProvider _algorithmProvider;
        public MembershipPasswordHasher(
               Pbkdf2HashStrategy pbkdf2,
               BCryptHashStrategy bcrypt,
               Argon2HashStrategy argon2,
               ICurrentAlgorithmProvider algorithmProvider)
        {
            _strategies = new Dictionary<PasswordHashAlgorithm, IPasswordHashStrategy>
            {
                [PasswordHashAlgorithm.Pbkdf2] = pbkdf2,
                [PasswordHashAlgorithm.BCrypt] = bcrypt,
                [PasswordHashAlgorithm.Argon2] = argon2
            };
            _algorithmProvider = algorithmProvider;
        }

        public string HashPassword(MembershipUser user, string password)
        {
            var algorithm = _algorithmProvider.GetCurrentAlgorithm();
            if (_strategies.TryGetValue(algorithm, out var strategy))
            {
                user.CurrentHashAlgorithm = algorithm;
                return strategy.Hash(password);
            }

            throw new NotSupportedException($"Hash algorithm not supported: {algorithm}");
        }

        public PasswordVerificationResult VerifyHashedPassword(MembershipUser user, string hashedPassword, string providedPassword)
        {
            if (user.CurrentHashAlgorithm is not PasswordHashAlgorithm algorithm) return PasswordVerificationResult.Failed;

            if (_strategies.TryGetValue(algorithm, out var strategy))
            {
                if (strategy.Verify(providedPassword, hashedPassword))
                {
                    if (algorithm != _algorithmProvider.GetCurrentAlgorithm())
                    {
                        return PasswordVerificationResult.SuccessRehashNeeded;
                    }
                    return PasswordVerificationResult.Success;
                }
            }

            return PasswordVerificationResult.Failed;
        }
    }
}
