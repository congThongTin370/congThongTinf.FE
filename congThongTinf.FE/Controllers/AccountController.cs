using congThongTinf.FE.Models;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class AccountController : Controller
    {
        public const string SessionAccessToken = "AccessToken";
        public const string SessionUserId = "UserId";
        public const string SessionFullName = "FullName";
        public const string SessionRole = "Role";

        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Password = string.Empty;
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Email, model.Password);

            model.Password = string.Empty;
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không đăng nhập được.");
                return View(model);
            }

            HttpContext.Session.SetString(SessionAccessToken, result.AccessToken);
            HttpContext.Session.SetString(SessionUserId, result.UserId.ToString());
            HttpContext.Session.SetString(SessionFullName, result.FullName);
            HttpContext.Session.SetString(SessionRole, result.Role);

            if (result.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
