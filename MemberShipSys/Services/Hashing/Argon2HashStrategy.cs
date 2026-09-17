//dotnet add package Konscious.Security.Cryptography.Argon2

using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace MemberShipSys.Services.Hashing
{
    public class Argon2HashStrategy : IPasswordHashStrategy
    {
        private const int HashSize = 32;
        private const int SaltSize = 16;

        public string Hash(string password)
        {
            int _iterations = 4; //（時間成本）：跑幾輪
            int _memorySize = 64 * 1024; // 64 MB （空間成本，單位 KB）：需要用掉多少記憶體
            int _degreeOfParallelism = 8; //平行執行緒數
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Create hash
            byte[] hash = HashPassword(password, salt, _iterations, _memorySize, _degreeOfParallelism);

            // Combine salt and hash
            var combinedBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, combinedBytes, 0, salt.Length);
            Array.Copy(hash, 0, combinedBytes, salt.Length, hash.Length);

            string result = $"{Convert.ToBase64String(combinedBytes)} ; {_iterations} ; {_memorySize} ; {_degreeOfParallelism}";

            return result;
        }

        private byte[] HashPassword(string password, byte[] salt, int iterations, int memorySize, int degreeOfParallelism)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize
            })
            return argon2.GetBytes(HashSize);
        }

        public bool Verify(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split(';');
            int iterationCount = int.Parse(parts[1]);
            int memorySize = int.Parse(parts[2]);
            int degreeOfParallelism = int.Parse(parts[3]);
            // Decode the stored hash
            byte[] combinedBytes = Convert.FromBase64String(parts[0]);

            // Extract salt and hash
            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[HashSize];
            Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
            Array.Copy(combinedBytes, SaltSize, hash, 0, HashSize);


            // Compute hash for the input password
            byte[] newHash = HashPassword(password, salt, iterationCount, memorySize, degreeOfParallelism);

            // Compare the hashes
            return CryptographicOperations.FixedTimeEquals(hash, newHash);
        }
    }
}
