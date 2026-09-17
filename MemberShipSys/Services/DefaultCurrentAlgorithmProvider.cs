using MemberShipSys.Models;

namespace MemberShipSys.Services
{

    public class DefaultCurrentAlgorithmProvider : ICurrentAlgorithmProvider
    {
        public PasswordHashAlgorithm GetCurrentAlgorithm() => PasswordHashAlgorithm.Argon2;
    }
    public interface ICurrentAlgorithmProvider
    {
        PasswordHashAlgorithm GetCurrentAlgorithm();
    }
}
