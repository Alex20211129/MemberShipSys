using MemberShipSys.Data;
using MemberShipSys.Models;
using System;

namespace MemberShipSys.Services
{
    public class DbCurrentAlgorithmProvider : ICurrentAlgorithmProvider
    {
        private readonly ApplicationDbContext _context;
        public DbCurrentAlgorithmProvider(ApplicationDbContext context)
        {
            _context = context;
        }
        public PasswordHashAlgorithm GetCurrentAlgorithm()
        {
            var setting = _context.AppSettings.First();
            return setting.CurrentHashAlgorithm;
        }
    }
}
