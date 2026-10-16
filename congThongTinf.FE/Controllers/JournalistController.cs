using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class JournalistController : Controller
    {
        private const string JournalistRole = "Journalist";
        private const long MaxCoverBytes = 10 * 1024 * 1024;
        private static readonly HashSet<string> CoverExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private readonly IJournalistNewsService _newsService;
        private readonly IMeService _meService;
        private readonly ICategoryService _categoryService;

        public JournalistController(
            IJournalistNewsService newsService,
            IMeService meService,
            ICategoryService categoryService)
        {
            _newsService = newsService;
            _meService = meService;
            _categoryService = categoryService;
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
            var news = await _newsService.GetMyNewsAsync(GetToken(), GetCurrentUserId());
            ViewBag.ArticleCount = news.Count;
            ViewBag.PostedCount = news.Count(item => NewsStatuses.IsPosted(item.Status));
            return View(user);
        }

        // Danh sách bài báo của chính phóng viên
        public async Task<IActionResult> Articles(
            DateTime? fromDate,
            DateTime? toDate,
            Guid? categoryId,
            int? status,
            string? search,
            int sortBy = 0)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Quản lý bài báo");

            if (status.HasValue && NewsStatuses.Options.All(option => option.Value != status.Value))
            {
                status = null;
            }

            if (sortBy != 1)
            {
                sortBy = 0;
            }

            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var model = new JournalistArticlesViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                CategoryId = categoryId,
                Status = status,
                Search = term,
                SortBy = sortBy,
                Categories = await _categoryService.GetActiveCategoriesAsync(),
                HasFilter = fromDate.HasValue || toDate.HasValue || categoryId.HasValue || status.HasValue || term != null
            };

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                model.LoadError = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.";
                return View(model);
            }

            var news = await _newsService.GetMyNewsAsync(GetToken(), GetCurrentUserId(), new NewsQueryDto
            {
                PageIndex = 0,
                FromDate = fromDate,
                ToDate = toDate,
                CategoryIds = categoryId.HasValue ? new List<Guid> { categoryId.Value } : null,
                Search = term,
                SortBy = sortBy
            });

            if (status.HasValue)
            {
                news = news.Where(item => NewsStatuses.MatchesFilter(item.Status, status.Value)).ToList();
            }

            model.Items = news;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Tạo bài viết mới");
            var model = new NewsFormViewModel();
            await PrepareNewsFormAsync(model);
            return View("NewsForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Create(NewsFormViewModel model)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Tạo bài viết mới");
            ValidateCoverImage(model.CoverImage);
            if (!ModelState.IsValid)
            {
                await PrepareNewsFormAsync(model);
                return View("NewsForm", model);
            }

            var request = ToWriteRequest(model);
            var result = await _newsService.CreateAsync(GetToken(), request);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không lưu được bài viết.");
                await PrepareNewsFormAsync(model);
                return View("NewsForm", model);
            }

            TempData["Message"] = "Đã đăng bài viết lên cổng thông tin.";
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
                CategoryName = news.CategoryName,
                StatusLabel = NewsStatuses.Label(news.Status)
            };
            await PrepareNewsFormAsync(model);
            return View("NewsForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Edit(Guid id, NewsFormViewModel model)
        {
            if (!IsJournalist())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Articles", "Sửa bài viết");
            model.Id = id;
            ValidateCoverImage(model.CoverImage);
            if (!ModelState.IsValid)
            {
                await PrepareNewsFormAsync(model);
                return View("NewsForm", model);
            }

            var request = ToWriteRequest(model);
            var result = await _newsService.UpdateAsync(GetToken(), id, request);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không cập nhật được bài viết.");
                await PrepareNewsFormAsync(model);
                return View("NewsForm", model);
            }

            TempData["Message"] = "Đã cập nhật bài viết.";
            return RedirectToAction(nameof(Articles));
        }

        // Upload ảnh chèn vào nội dung bài. Token nằm trong session phía server, không trả về trình duyệt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile? file)
        {
            if (!IsJournalist())
            {
                return Json(new { success = false, message = UserFacingErrors.NoPermission });
            }

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ảnh hợp lệ." });
            }

            var result = await _newsService.UploadContentImageAsync(GetToken(), file);
            if (!result.Success || string.IsNullOrWhiteSpace(result.Url))
            {
                return Json(new { success = false, message = result.ErrorMessage ?? UserFacingErrors.TryAgain });
            }

            return Json(new { success = true, url = result.Url });
        }

        // Xem trước nội dung: render Markdown giống trang chi tiết (đã làm sạch HTML)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PreviewContent(string? content)
        {
            if (!IsJournalist())
            {
                return Json(new { success = false, message = UserFacingErrors.NoPermission });
            }

            return Json(new { success = true, html = HtmlSanitizer.RenderMarkdown(content) });
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

        private static NewsWriteRequestDto ToWriteRequest(NewsFormViewModel model) => new()
        {
            Title = model.Title,
            Summary = model.Summary ?? string.Empty,
            Content = model.Content,
            CategoryId = model.CategoryId!.Value,
            CoverImage = model.CoverImage
        };

        private void ValidateCoverImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return;
            }

            if (file.Length > MaxCoverBytes)
            {
                ModelState.AddModelError(nameof(NewsFormViewModel.CoverImage), "Ảnh bìa không được lớn hơn 10 MB.");
                return;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !CoverExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(NewsFormViewModel.CoverImage), "Ảnh bìa phải là jpg, jpeg, png, webp hoặc gif.");
            }
        }

        private async Task PrepareNewsFormAsync(NewsFormViewModel model)
        {
            model.Categories = await _categoryService.GetActiveCategoriesAsync();
            model.AuthorName = HttpContext.Session.GetString(AccountController.SessionFullName) ?? "Phóng viên";
            if (string.IsNullOrWhiteSpace(model.StatusLabel))
            {
                model.StatusLabel = NewsStatuses.PostedLabel;
            }
        }

        private void SetLayoutData(string activeMenu, string title)
        {
            ViewData["Title"] = title;
            ViewData["ActiveMenu"] = activeMenu;
            ViewData["FullName"] = HttpContext.Session.GetString(AccountController.SessionFullName) ?? "Phóng viên";
        }
    }
}
