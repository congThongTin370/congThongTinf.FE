using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    /// <summary>
    /// Lưu trữ trực tuyến: file chia sẻ cho user đã đăng nhập. Gọi /api/shared-files bằng JWT trong session.
    /// </summary>
    public class SharedFilesController : Controller
    {
        private const long MaxUploadBytes = 10 * 1024 * 1024;

        private readonly ISharedFileService _sharedFileService;

        public SharedFilesController(ISharedFileService sharedFileService)
        {
            _sharedFileService = sharedFileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, string? search = null)
        {
            if (!IsLoggedIn())
            {
                return RedirectToLogin();
            }

            SetLayoutData("Lưu trữ trực tuyến");
            var model = await _sharedFileService.GetMyFilesAsync(GetToken(), Math.Max(pageIndex, 1), 10, search);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<IActionResult> Upload(SharedFileUploadViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToLogin();
            }

            if (model.File != null && model.File.Length > MaxUploadBytes)
            {
                TempData["Error"] = "File không được lớn hơn 10 MB.";
                return RedirectToAction(nameof(Index));
            }

            if (model.File == null || model.File.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file cần tải lên.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _sharedFileService.UploadAsync(GetToken(), model.File);
            return FinishWrite(result, "Đã tải file lên.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(Guid id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToLogin();
            }

            var result = await _sharedFileService.CreateShareLinkAsync(GetToken(), id);
            if (result.Success)
            {
                TempData["Message"] = "Đã tạo liên kết chia sẻ.";
                return RedirectToAction(nameof(Index), new { shared = id });
            }

            return FinishWrite(result, "Đã tạo liên kết chia sẻ.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(Guid id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToLogin();
            }

            var result = await _sharedFileService.RevokeShareLinkAsync(GetToken(), id);
            return FinishWrite(result, "Đã thu hồi liên kết chia sẻ.");
        }

        /// <summary>
        /// Trang người nhận mở link chia sẻ. Yêu cầu đăng nhập; token không lộ ra cho người nhận.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Link(string token)
        {
            if (!IsLoggedIn())
            {
                return RedirectToLogin();
            }

            SetLayoutData("Mở file chia sẻ");
            var result = await _sharedFileService.GetByLinkTokenAsync(GetToken(), token);
            if (result.SessionExpired)
            {
                return RedirectToLogin();
            }

            return View(new SharedFileOpenViewModel
            {
                File = result.Data,
                ErrorMessage = result.ErrorMessage
            });
        }

        /// <summary>
        /// Stream file (xem inline hoặc tải về) qua FE, đính kèm JWT. Không để lộ URL R2.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Stream(string url, bool download = false)
        {
            if (!IsLoggedIn())
            {
                return RedirectToLogin();
            }

            // Chỉ cho phép gọi về API backend của chính FE, không proxy URL tùy ý
            if (!IsAllowedBackendUrl(url))
            {
                return NotFound();
            }

            var result = await _sharedFileService.OpenStreamAsync(GetToken(), url);
            if (result.SessionExpired)
            {
                return RedirectToLogin();
            }

            if (!result.Success || result.Content == null)
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
                return RedirectToAction(nameof(Index));
            }

            // Inline (xem) hoặc attachment (tải). fileDownloadName chỉ dùng khi tải.
            var fileName = result.FileName ?? "file";
            if (download)
            {
                return File(result.Content, result.ContentType, fileName);
            }

            Response.Headers.ContentDisposition = $"inline; filename=\"{fileName}\"";
            return File(result.Content, result.ContentType);
        }

        private IActionResult FinishWrite(SharedFileWriteResult result, string successMessage)
        {
            if (result.SessionExpired)
            {
                return RedirectToLogin();
            }

            TempData[result.Success ? "Message" : "Error"] = result.Success
                ? successMessage
                : result.ErrorMessage ?? UserFacingErrors.TryAgain;
            return RedirectToAction(nameof(Index));
        }

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString(AccountController.SessionAccessToken));

        private string GetToken() =>
            HttpContext.Session.GetString(AccountController.SessionAccessToken) ?? string.Empty;

        private IActionResult RedirectToLogin()
        {
            TempData["Error"] = UserFacingErrors.SessionExpired;
            return RedirectToAction("Login", "Account");
        }

        private bool IsAllowedBackendUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var baseUri = new Uri(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"]
                ?? "http://localhost:5000");
            return uri.Host.Equals(baseUri.Host, StringComparison.OrdinalIgnoreCase)
                && uri.Port == baseUri.Port
                && uri.AbsolutePath.StartsWith("/api/shared-files", StringComparison.OrdinalIgnoreCase);
        }

        private void SetLayoutData(string title)
        {
            ViewData["Title"] = title;
        }
    }
}
