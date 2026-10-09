using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString(AccountController.SessionRole) != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Bảng điều khiển";
            ViewData["ActiveMenu"] = "Dashboard";
            ViewData["FullName"] = HttpContext.Session.GetString(AccountController.SessionFullName);
            return View();
        }
    }
}
