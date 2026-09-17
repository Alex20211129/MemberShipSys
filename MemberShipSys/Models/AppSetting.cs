namespace MemberShipSys.Models
{
    public class AppSetting
    {
        public int Id { get; set; }
        public PasswordHashAlgorithm CurrentHashAlgorithm { get; set; }
    }
}
