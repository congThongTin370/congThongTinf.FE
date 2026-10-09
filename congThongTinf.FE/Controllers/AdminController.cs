using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // Chỉ Admin được vào dashboard; role khác chuyển về trang chủ
            if (HttpContext.Session.GetString(AccountController.SessionRole) != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Bảng điều khiển";
            ViewData["ActiveMenu"] = "Dashboard";
            ViewData["FullName"] = HttpContext.Session.GetString(AccountController.SessionFullName) ?? "Quản trị viên";
            return View();
        }
    }
}
