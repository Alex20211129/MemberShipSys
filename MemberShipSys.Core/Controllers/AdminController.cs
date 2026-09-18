using MemberShipSys.Data;
using MemberShipSys.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MemberShipSys.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<MembershipUser> _userManager;
        private readonly ApplicationDbContext _context;
        public AdminController(UserManager<MembershipUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            List<MemberListItemViewModel> userViewModels = new List<MemberListItemViewModel>();
            foreach (var user in users)
            {
                userViewModels.Add(new MemberListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    IsDisabled = user.IsDisabled,
                    IsLockedOut = await _userManager.IsLockedOutAsync(user),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    LoginCount = user.LoginCount,
                    Roles = await _userManager.GetRolesAsync(user),
                    CurrentHashAlgorithm = user.CurrentHashAlgorithm,
                });
            }
            return View(userViewModels);
        }

        //啟用 || 停用
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDisabled(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            user.IsDisabled = !user.IsDisabled;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        //解除鎖定
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                // 目標是自己，不允許操作，直接導回列表
                return RedirectToAction(nameof(Index));
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "Member");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Member");
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceLogout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            // 這裡的邏輯是將使用者的安全戳記更新，迫使所有現有的登入會話失效
            await _userManager.UpdateSecurityStampAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var setting = await _context.AppSettings.FirstAsync();
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(PasswordHashAlgorithm currentHashAlgorithm)
        {
            var setting = await _context.AppSettings.FirstAsync();
            setting.CurrentHashAlgorithm = currentHashAlgorithm;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Settings));
        }
    }
}
