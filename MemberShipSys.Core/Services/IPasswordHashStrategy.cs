namespace MemberShipSys.Services
{
    public interface IPasswordHashStrategy
    {
        string Hash (string password);
        bool Verify(string password, string hashedPassword);
    }
}
