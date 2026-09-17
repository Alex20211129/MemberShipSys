using MemberShipSys.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MemberShipSys.Data
{
    public class ApplicationDbContext : IdentityDbContext<MembershipUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<AppSetting> AppSettings { get; set; }
    }
}
