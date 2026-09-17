using MemberShipSys.Models;
using MemberShipSys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
namespace MemberShipSys.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<MembershipUser> _userManager;
        private readonly SignInManager<MembershipUser> _signInManager;
        private readonly IEmailSender _emailSender;
        public AccountController(UserManager<MembershipUser> userManager, SignInManager<MembershipUser> signInManager, IConfiguration configuration, IHttpClientFactory httpClientFactory, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _emailSender = emailSender;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new MembershipUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!addToRoleResult.Succeeded)
            {
                foreach (var error in addToRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "請驗證您的信箱",
                    $"<p>請點擊以下連結來驗證您的信箱：<a href='{confirmLink}'>驗證信箱</a></p>"
                );
            }
            catch (Exception)
            {
                TempData["EmailSendError"] = "帳號建立成功，但驗證信寄送失敗，請稍後再試或聯絡管理員。";
            }
            return RedirectToAction("RegisterConfirmation");
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
        {
            if (userId is null || token is null)
            {
                return View("ConfirmEmailFailed");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return View("ConfirmEmailFailed");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return View("ConfirmEmailFailed");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            //不成功的情況下，需判斷是鎖住還是帳密錯誤
            if (!result.Succeeded)
            {
                switch (result)
                {
                    case { IsLockedOut: true }:
                        ModelState.AddModelError(string.Empty, "帳號已被鎖定，請稍後再試。");
                        break;
                    default:
                        ModelState.AddModelError(string.Empty, "登入失敗，請檢查您的帳號和密碼。");
                        break;
                }
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null && user.IsDisabled)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "帳號已被停用，請聯絡管理員。");
                return View(model);
            }


            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var state = Guid.NewGuid().ToString("N");
            Response.Cookies.Append("GoogleOAuthState", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            });

            var redirectUri = Url.Action("GoogleCallback", "Account", null, Request.Scheme);

            var queryParams = new Dictionary<string, string?>
            {
                ["client_id"] = _configuration["GoogleOAuth:ClientId"],
                ["redirect_uri"] = redirectUri,
                ["response_type"] = "code",
                ["scope"] = "openid email profile",
                ["state"] = state,
            };

            var authorizationUrl = QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", queryParams);
            return Redirect(authorizationUrl);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? code, string? state, string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return RedirectToAction("Login");
            }

            var savedState = Request.Cookies["GoogleOAuthState"];
            Response.Cookies.Delete("GoogleOAuthState");

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state) || state != savedState)
            {
                return BadRequest("Invalid OAuth state.");
            }

            var redirectUri = Url.Action("GoogleCallback", "Account", null, Request.Scheme);
            var httpClient = _httpClientFactory.CreateClient();

            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _configuration["GoogleOAuth:ClientId"]!,
                ["client_secret"] = _configuration["GoogleOAuth:ClientSecret"]!,
                ["redirect_uri"] = redirectUri!,
                ["grant_type"] = "authorization_code"
            }));
            tokenResponse.EnsureSuccessStatusCode();
            var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            var accessToken = tokenJson.GetProperty("access_token").GetString();

            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var userInfoResponse = await httpClient.SendAsync(userInfoRequest);
            userInfoResponse.EnsureSuccessStatusCode();
            var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>();

            var googleUserId = userInfo.GetProperty("sub").GetString()!;
            var email = userInfo.GetProperty("email").GetString()!;

            var user = await _userManager.FindByLoginAsync("Google", googleUserId);
            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    user = new MembershipUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest("無法建立帳號：" + string.Join("; ", createResult.Errors.Select(e => e.Description)));
                    }
                    await _userManager.AddToRoleAsync(user, "Member");
                }

                await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleUserId, "Google"));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            if (await _userManager.HasPasswordAsync(user))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var result = await _userManager.AddPasswordAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ResendConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                try
                {
                    await _emailSender.SendEmailAsync(
                        user.Email!,
                        "請驗證您的信箱",
                        $"<p>請點擊以下連結完成信箱驗證：</p><a href='{confirmLink}'>{confirmLink}</a>");
                }
                catch (Exception)
                {
                    // 這裡故意不處理錯誤訊息、不讓畫面知道寄信是否成功——原因「不洩漏帳號是否存在」
                }
            }
            TempData["StatusMessage"] = "如果這個信箱有對應帳號，我們已經寄出新的驗證信，請查收。";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, token }, Request.Scheme);
                try
                {
                    await _emailSender.SendEmailAsync(
                        user.Email!,
                        "重設您的密碼",
                        $"<p>請點擊以下連結完成密碼重設：</p><a href='{resetLink}'>{resetLink}</a>");
                }
                catch (Exception)
                {
                    // 這裡故意不處理錯誤訊息、不讓畫面知道寄信是否成功——原因「不洩漏帳號是否存在」
                }
            }
            TempData["StatusMessage"] = "如果這個信箱有對應帳號，我們已經寄出重設密碼的信件，請查收。";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string? userId, string? token)
        {
            if (userId is null || token is null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            TempData["StatusMessage"] = "密碼已重設，請用新密碼登入。";
            return RedirectToAction("Login");
        }

    }


}
