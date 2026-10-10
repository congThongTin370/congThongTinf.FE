using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    public class AdminController : Controller
    {
        private const string AdminRole = "Admin";
        private const long MaxUploadBytes = 10 * 1024 * 1024;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt"
        };

        private readonly IDocumentService _documentService;

        public AdminController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Dashboard", "Bảng điều khiển");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Documents(string? search, int? category, int pageIndex = 1)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Documents", "Tài liệu nội bộ");

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            if (category.HasValue && DocumentCategories.Options.All(o => o.Value != category.Value))
            {
                category = null;
            }

            var items = await _documentService.GetDocumentsAsync(new DocumentQuery
            {
                PageIndex = pageIndex,
                PageSize = 10,
                Search = search,
                Category = category
            });

            return View(new DocumentListViewModel
            {
                Items = items,
                Search = search,
                Category = category,
                PageIndex = pageIndex,
                PageSize = 10
            });
        }

        [HttpGet]
        public IActionResult UploadDocument()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Documents", "Tải lên tài liệu");
            return View(new DocumentUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(11 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
        public async Task<IActionResult> UploadDocument(DocumentUploadViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Documents", "Tải lên tài liệu");
            ValidateUpload(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _documentService.UploadAsync(GetToken(), model);
            if (result.SessionExpired)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không tải lên được tài liệu.");
                return View(model);
            }

            TempData["Message"] = "Đã tải tài liệu nội bộ lên cổng thông tin.";
            return RedirectToAction(nameof(Documents));
        }

        private void ValidateUpload(DocumentUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError(nameof(model.File), "Chọn tệp tài liệu.");
                return;
            }

            if (model.File.Length > MaxUploadBytes)
            {
                ModelState.AddModelError(nameof(model.File), "Tệp không được lớn hơn 10 MB.");
            }

            var extension = Path.GetExtension(model.File.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.File), "Chỉ nhận tệp pdf, doc, docx, xls, xlsx, ppt, pptx hoặc txt.");
            }

            if (DocumentCategories.Options.All(o => o.Value != model.Category))
            {
                ModelState.AddModelError(nameof(model.Category), "Danh mục tài liệu chưa đúng. Vui lòng chọn lại.");
            }
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString(AccountController.SessionRole) == AdminRole;
        }

        private string GetToken()
        {
            return HttpContext.Session.GetString(AccountController.SessionAccessToken) ?? string.Empty;
        }

        private void SetLayoutData(string activeMenu, string title)
        {
            ViewData["Title"] = title;
            ViewData["ActiveMenu"] = activeMenu;
            ViewData["FullName"] = HttpContext.Session.GetString(AccountController.SessionFullName) ?? "Quản trị viên";
        }
    }
}
