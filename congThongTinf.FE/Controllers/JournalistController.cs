using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class JournalistController : Controller
    {
        private const string JournalistRole = "Journalist";

        // Trang mặc định của phóng viên: thông tin cá nhân chi tiết
        public IActionResult Profile()
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Profile", "Trang cá nhân");
            return View();
        }

        // UI CRUD bài báo
        public IActionResult Articles()
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Quản lý bài báo");
            return View();
        }

        private bool IsJournalist()
        {
            return HttpContext.Session.GetString(AccountController.SessionRole) == JournalistRole;
        }

        private void SetLayoutData(string activeMenu, string title)
        {
            ViewData["Title"] = title;
            ViewData["ActiveMenu"] = activeMenu;
            ViewData["FullName"] = HttpContext.Session.GetString(AccountController.SessionFullName) ?? "Phóng viên";
        }
    }
}
