using congThongTinf.FE.Models;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ModelState.Remove(nameof(model.Password));
            model.Password = string.Empty;
            ModelState.AddModelError(string.Empty, "Không đăng nhập được vì cổng thông tin chưa kết nối hệ thống xác thực.");
            return View(model);
        }
    }
}
