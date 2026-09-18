using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace MemberShipSys.Services.Hashing
{
    public class Pbkdf2HashStrategy : IPasswordHashStrategy
    {
        public string Hash(string password)
        {
            int iterationCount = 100000;
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterationCount,
                numBytesRequested: 256 / 8));
            // 將 salt 以 Base64 儲存，方便之後從字串還原為 byte[]
            return $"{hashed};{Convert.ToBase64String(salt)};{iterationCount}";
        }

        public bool Verify(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split(';');
            string hashed = parts[0];
            byte[] salt = Convert.FromBase64String(parts[1]);
            int iterationCount = int.Parse(parts[2]);

            byte[] candidateBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,   
                iterationCount: iterationCount,
                numBytesRequested: 256 / 8);

            // 轉回 byte[] 再比較，符合 FixedTimeEquals 的參數型別
            byte[] hashedBytes = Convert.FromBase64String(hashed);

            // 不使用 == 比對，使用 FixedTimeEquals 進行安全的比較，避免時間攻擊
            // ("回應時間差異" 慢慢猜出正確的 byte，這叫 timing attack；FixedTimeEquals保證固定時間，不會洩漏資訊。)
            return CryptographicOperations.FixedTimeEquals(hashedBytes, candidateBytes);
        }
    }
}
