
//dotnet add package BCrypt.Net-Next
using BCrypt.Net;

namespace MemberShipSys.Services.Hashing
{
    public class BCryptHashStrategy : IPasswordHashStrategy
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
