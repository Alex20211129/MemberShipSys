using MemberShipSys.Models;

namespace MemberShipSys.Services
{
    public interface ICurrentAlgorithmProvider
    {
        PasswordHashAlgorithm GetCurrentAlgorithm();
    }
}
