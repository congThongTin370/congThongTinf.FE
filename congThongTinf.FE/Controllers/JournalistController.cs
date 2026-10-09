using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class JournalistController : Controller
    {
        private const string JournalistRole = "Journalist";

        private readonly IJournalistNewsService _newsService;
        private readonly IMeService _meService;

        public JournalistController(IJournalistNewsService newsService, IMeService meService)
        {
            _newsService = newsService;
            _meService = meService;
        }

        // Trang mặc định của phóng viên: thông tin cá nhân chi tiết
        public async Task<IActionResult> Profile()
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Profile", "Trang cá nhân");
            var user = await _meService.GetCurrentUserDetailsAsync(GetToken());
            return View(user);
        }

        // Danh sách bài báo của chính phóng viên
        public async Task<IActionResult> Articles()
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Quản lý bài báo");
            var authorId = GetCurrentUserId();
            var news = await _newsService.GetMyNewsAsync(GetToken(), authorId);
            return View(news);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Viết bài mới");
            return View("NewsForm", new NewsFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsFormViewModel model)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Viết bài mới");
            if (!ModelState.IsValid)
            {
                return View("NewsForm", model);
            }

            var request = new CreateNewsRequestDto
            {
                Title = model.Title,
                Summary = model.Summary ?? string.Empty,
                Content = model.Content,
                CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl) ? null : model.CoverImageUrl,
                CategoryId = model.CategoryId!.Value
            };

            var result = await _newsService.CreateAsync(GetToken(), request);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không lưu được bài viết.");
                return View("NewsForm", model);
            }

            TempData["Message"] = "Đã tạo bài viết (bản nháp).";
            return RedirectToAction(nameof(Articles));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Sửa bài viết");
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return RedirectToAction(nameof(Articles));
            }

            // Chỉ tác giả mới được sửa
            if (news.AuthorId != GetCurrentUserId())
            {
                return RedirectToAction(nameof(Articles));
            }

            var model = new NewsFormViewModel
            {
                Id = news.Id,
                Title = news.Title ?? string.Empty,
                Summary = news.Summary,
                Content = news.Content ?? string.Empty,
                CoverImageUrl = news.CoverImageUrl,
                CategoryId = news.CategoryId,
                StatusLabel = StatusLabel(news.Status)
            };
            return View("NewsForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, NewsFormViewModel model)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Sửa bài viết");
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View("NewsForm", model);
            }

            var request = new UpdateNewsRequestDto
            {
                Title = model.Title,
                Summary = model.Summary,
                Content = model.Content,
                CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl) ? null : model.CoverImageUrl,
                CategoryId = model.CategoryId
            };

            var result = await _newsService.UpdateAsync(GetToken(), id, request);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không cập nhật được bài viết.");
                return View("NewsForm", model);
            }

            TempData["Message"] = "Đã cập nhật bài viết.";
            return RedirectToAction(nameof(Articles));
        }

        // Bước 1: hỏi xác nhận trước khi xóa. Chưa xóa gì ở bước này.
        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(Guid id)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Xác nhận xóa bài viết");
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null || news.AuthorId != GetCurrentUserId())
            {
                return RedirectToAction(nameof(Articles));
            }

            return View(news);
        }

        // Bước 2: chỉ xóa khi người dùng đã xác nhận ở trang trên
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteConfirm")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _newsService.DeleteAsync(GetToken(), id);
            TempData["Message"] = result.Success ? "Đã xóa bài viết." : result.ErrorMessage;
            return RedirectToAction(nameof(Articles));
        }

        private bool IsJournalist()
        {
            return HttpContext.Session.GetString(AccountController.SessionRole) == JournalistRole;
        }

        private string GetToken()
        {
            return HttpContext.Session.GetString(AccountController.SessionAccessToken) ?? string.Empty;
        }

        private Guid GetCurrentUserId()
        {
            return Guid.TryParse(HttpContext.Session.GetString(AccountController.SessionUserId), out var id)
                ? id
                : Guid.Empty;
        }

        private static string StatusLabel(int status) => status switch
        {
            0 => "Bản nháp",
            1 => "Chờ duyệt",
            2 => "Đã xuất bản",
            3 => "Lưu trữ",
            _ => "Không xác định"
        };

        private void SetLayoutData(string activeMenu, string title)
        {
            ViewData["Title"] = title;
            ViewData["ActiveMenu"] = activeMenu;
            ViewData["FullName"] = HttpContext.Session.GetString(AccountController.SessionFullName) ?? "Phóng viên";
        }
    }
}
