using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;
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

        private static readonly HashSet<string> AllowedBannerExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly INewsService _newsService;
        private readonly IAdminUserService _userService;
        private readonly IDashboardService _dashboardService;
        private readonly IBannerService _bannerService;

        public AdminController(
            IDocumentService documentService,
            ICategoryService categoryService,
            INewsService newsService,
            IAdminUserService userService,
            IDashboardService dashboardService,
            IBannerService bannerService)
        {
            _documentService = documentService;
            _categoryService = categoryService;
            _newsService = newsService;
            _userService = userService;
            _dashboardService = dashboardService;
            _bannerService = bannerService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Dashboard", "Bảng điều khiển");

            var today = DateTime.UtcNow.Date;
            var model = new AdminDashboardViewModel
            {
                FromDate = fromDate?.Date ?? today.AddDays(-29),
                ToDate = toDate?.Date ?? today
            };

            if (model.FromDate > model.ToDate)
            {
                model.LoadError = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.";
                return View(model);
            }

            var result = await _dashboardService.GetDashboardAsync(GetToken(), model.FromDate, model.ToDate);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (!result.Success)
            {
                model.LoadError = result.ErrorMessage ?? UserFacingErrors.TryAgain;
                return View(model);
            }

            model.Data = result.Data;
            return View(model);
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

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Categories", "Danh mục");
            var items = await _categoryService.GetActiveCategoriesAsync();
            return View(items);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Categories", "Tạo danh mục");
            return View("CategoryForm", new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryFormViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Categories", "Tạo danh mục");
            if (!ModelState.IsValid)
            {
                return View("CategoryForm", model);
            }

            var result = await _categoryService.CreateAsync(GetToken(), model);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? UserFacingErrors.TryAgain);
                return View("CategoryForm", model);
            }

            TempData["Message"] = "Đã tạo danh mục.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Categories", "Sửa danh mục");
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục này.";
                return RedirectToAction(nameof(Categories));
            }

            return View("CategoryForm", new CategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Guid id, CategoryFormViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Categories", "Sửa danh mục");
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View("CategoryForm", model);
            }

            var result = await _categoryService.UpdateAsync(GetToken(), id, model);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? UserFacingErrors.TryAgain);
                return View("CategoryForm", model);
            }

            TempData["Message"] = "Đã cập nhật danh mục.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Categories", "Xác nhận xóa danh mục");
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục này.";
                return RedirectToAction(nameof(Categories));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteCategory")]
        public async Task<IActionResult> DeleteCategoryConfirmed(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _categoryService.DeleteAsync(GetToken(), id);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (result.Success)
            {
                TempData["Message"] = "Đã xóa danh mục.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> News(
            DateTime? fromDate,
            DateTime? toDate,
            Guid? categoryId,
            int? status,
            string? search,
            int pageIndex = 1)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("News", "Quản lý tin tức");

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            if (status.HasValue && NewsStatuses.Options.All(option => option.Value != status.Value))
            {
                status = null;
            }

            var pageSize = 10;
            var model = new AdminNewsListViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                CategoryId = categoryId,
                Status = status,
                Search = search,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Categories = await _categoryService.GetActiveCategoriesAsync()
            };

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                model.LoadError = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.";
                return View(model);
            }

            // API lọc tiêu đề qua search. Trạng thái không có trên query nên vẫn lọc phía FE.
            var filterStatusOnClient = status.HasValue;
            List<NewsDto> news;
            try
            {
                news = await _newsService.GetAllNewsAsync(new NewsQueryDto
                {
                    PageIndex = filterStatusOnClient ? 0 : pageIndex,
                    PageSize = filterStatusOnClient ? null : pageSize,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CategoryIds = categoryId.HasValue ? new List<Guid> { categoryId.Value } : null,
                    Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim()
                });
            }
            catch (Exception)
            {
                model.LoadError = UserFacingErrors.LoadNewsFailed;
                return View(model);
            }

            if (filterStatusOnClient)
            {
                var matched = news
                    .Where(item => NewsStatuses.MatchesFilter(item.Status, status!.Value))
                    .ToList();
                SetStatusCounts(model, matched);
                model.Items = matched.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                model.HasNext = pageIndex * pageSize < matched.Count;
            }
            else
            {
                SetStatusCounts(model, news);
                model.StatusCountsFromCurrentPage = true;
                model.Items = news;
                model.HasNext = news.Count >= pageSize;
            }

            model.HasPrevious = pageIndex > 1;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> NewsDetails(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("News", "Chi tiết tin tức");

            try
            {
                var news = await _newsService.GetNewsByIdAsync(id);
                return View(news);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = UserFacingErrors.LoadNewsFailed;
                return RedirectToAction(nameof(News));
            }
            catch (Exception)
            {
                TempData["Error"] = "Không tìm thấy bài viết này.";
                return RedirectToAction(nameof(News));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Users(string? search, int? roleName, bool? isActive, int pageIndex = 1)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Users", "Quản lý người dùng");

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            if (roleName.HasValue && UserRoles.Options.All(option => option.Value != roleName.Value))
            {
                roleName = null;
            }

            var model = new AdminUserListViewModel
            {
                Search = search,
                RoleName = roleName,
                IsActive = isActive,
                PageIndex = pageIndex,
                PageSize = 10,
                CurrentUserId = GetCurrentUserId()
            };

            var result = await _userService.GetUsersAsync(GetToken(), new AdminUserQuery
            {
                PageIndex = pageIndex,
                PageSize = 10,
                Search = search,
                RoleName = roleName,
                IsActive = isActive
            });

            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (!result.Success)
            {
                model.LoadError = result.ErrorMessage ?? UserFacingErrors.TryAgain;
                return View(model);
            }

            model.Items = result.Items;
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Users", "Tạo tài khoản");
            return View("UserForm", new UserFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserFormViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Users", "Tạo tài khoản");
            ValidateUserForm(model, requirePassword: true);
            if (!ModelState.IsValid)
            {
                return View("UserForm", model);
            }

            var result = await _userService.CreateAsync(GetToken(), model);
            var failed = HandleUserWrite(result);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? UserFacingErrors.TryAgain);
                return View("UserForm", model);
            }

            TempData["Message"] = "Đã tạo tài khoản.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Users", "Sửa tài khoản");
            var result = await _userService.GetByIdAsync(GetToken(), id);
            var failed = HandleUserWrite(result, redirectToList: true);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.ErrorMessage ?? "Không tìm thấy người dùng này.";
                return RedirectToAction(nameof(Users));
            }

            var user = result.Data;
            return View("UserForm", new UserFormViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.RoleName,
                IsActive = user.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(Guid id, UserFormViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Users", "Sửa tài khoản");
            model.Id = id;
            ValidateUserForm(model, requirePassword: false);
            if (!ModelState.IsValid)
            {
                return View("UserForm", model);
            }

            var result = await _userService.UpdateAsync(GetToken(), id, model);
            var failed = HandleUserWrite(result);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? UserFacingErrors.TryAgain);
                return View("UserForm", model);
            }

            TempData["Message"] = "Đã cập nhật tài khoản.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == GetCurrentUserId())
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Users));
            }

            SetLayoutData("Users", "Xác nhận khóa tài khoản");
            var result = await _userService.GetByIdAsync(GetToken(), id);
            var failed = HandleUserWrite(result, redirectToList: true);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.ErrorMessage ?? "Không tìm thấy người dùng này.";
                return RedirectToAction(nameof(Users));
            }

            if (!result.Data.IsActive)
            {
                TempData["Error"] = "Tài khoản này đã bị khóa.";
                return RedirectToAction(nameof(Users));
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == GetCurrentUserId())
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userService.DeleteAsync(GetToken(), id);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (result.Success)
            {
                TempData["Message"] = "Đã khóa tài khoản.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> Banners(string? search, int? position)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Banners", "Quản lý banner");
            var slot = position.HasValue && BannerPositions.IsValid(position.Value) ? position : null;

            var model = new BannerListViewModel
            {
                Search = search,
                Position = slot,
                PageIndex = 0,
                PageSize = 10
            };

            var result = await _bannerService.GetBannersAsync(GetToken(), new BannerQuery
            {
                PageIndex = 0,
                PageSize = 10
            });

            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (!result.Success)
            {
                model.LoadError = result.ErrorMessage ?? UserFacingErrors.TryAgain;
                return View(model);
            }

            model.Items = result.Items;
            model.TotalCount = result.TotalCount;
            model.HasPrevious = result.HasPrevious;
            model.HasNext = result.HasNext;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetBannerActive(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _bannerService.SetActiveAsync(GetToken(), id);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (result.Success)
            {
                TempData["Message"] = "Đã đặt banner hiển thị cho vị trí này.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
            }

            return RedirectToAction(nameof(Banners), new { position = result.Data?.Position });
        }

        [HttpGet]
        public IActionResult CreateBanner(int? position)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Banners", "Tải banner");
            var model = new BannerFormViewModel();
            if (position.HasValue && BannerPositions.IsValid(position.Value))
            {
                model.Position = position.Value;
            }

            return View("BannerForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(11 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
        public async Task<IActionResult> CreateBanner(BannerFormViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Banners", "Tải banner");
            ValidateBanner(model, requireFile: true);
            if (!ModelState.IsValid)
            {
                return View("BannerForm", model);
            }

            var result = await _bannerService.CreateAsync(GetToken(), model);
            var failed = HandleBannerWrite(result);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? UserFacingErrors.TryAgain);
                return View("BannerForm", model);
            }

            TempData["Message"] = "Đã tải banner lên cổng thông tin.";
            return RedirectToAction(nameof(Banners), new { position = model.Position });
        }

        [HttpGet]
        public async Task<IActionResult> EditBanner(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Banners", "Sửa banner");
            var result = await _bannerService.GetByIdAsync(GetToken(), id);
            var failed = HandleBannerWrite(result, redirectToList: true);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.ErrorMessage ?? "Không tìm thấy banner này.";
                return RedirectToAction(nameof(Banners));
            }

            var banner = result.Data;
            return View("BannerForm", new BannerFormViewModel
            {
                Id = banner.Id,
                Title = banner.Title,
                LinkUrl = banner.LinkUrl,
                Position = banner.Position,
                IsActive = banner.IsActive,
                CurrentImageUrl = banner.ImageUrl,
                CurrentFileName = banner.FileName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(11 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
        public async Task<IActionResult> EditBanner(Guid id, BannerFormViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Banners", "Sửa banner");
            model.Id = id;
            ValidateBanner(model, requireFile: false);
            if (!ModelState.IsValid)
            {
                return View("BannerForm", model);
            }

            var result = await _bannerService.UpdateAsync(GetToken(), id, model);
            var failed = HandleBannerWrite(result);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? UserFacingErrors.TryAgain);
                return View("BannerForm", model);
            }

            TempData["Message"] = "Đã cập nhật banner.";
            return RedirectToAction(nameof(Banners), new { position = model.Position });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBanner(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            SetLayoutData("Banners", "Xác nhận xóa banner");
            var result = await _bannerService.GetByIdAsync(GetToken(), id);
            var failed = HandleBannerWrite(result, redirectToList: true);
            if (failed != null)
            {
                return failed;
            }

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.ErrorMessage ?? "Không tìm thấy banner này.";
                return RedirectToAction(nameof(Banners));
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteBanner")]
        public async Task<IActionResult> DeleteBannerConfirmed(Guid id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _bannerService.DeleteAsync(GetToken(), id);
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (result.Success)
            {
                TempData["Message"] = "Đã xóa banner.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
            }

            return RedirectToAction(nameof(Banners));
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

        private void ValidateBanner(BannerFormViewModel model, bool requireFile)
        {
            var hasFile = model.File != null && model.File.Length > 0;
            if (requireFile && !hasFile)
            {
                ModelState.AddModelError(nameof(model.File), "Chọn ảnh banner.");
            }

            if (hasFile && model.File != null)
            {
                if (model.File.Length > MaxUploadBytes)
                {
                    ModelState.AddModelError(nameof(model.File), "Ảnh banner không được lớn hơn 10 MB.");
                }

                var extension = Path.GetExtension(model.File.FileName);
                if (!AllowedBannerExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.File), "Banner phải là ảnh jpg, jpeg, png, webp hoặc gif.");
                }
            }

            if (!requireFile && string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError(nameof(model.Title), "Tiêu đề banner là bắt buộc.");
            }

            if (!BannerPositions.IsValid(model.Position))
            {
                ModelState.AddModelError(nameof(model.Position), "Vị trí banner không hợp lệ.");
            }

            if (model.LinkUrl != null && model.LinkUrl.Trim().Length > 1000)
            {
                ModelState.AddModelError(nameof(model.LinkUrl), "Link banner không được vượt quá 1000 ký tự.");
            }
        }

        private IActionResult? HandleBannerWrite(BannerWriteResult result, bool redirectToList = false)
        {
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (redirectToList && !result.Success)
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
                return RedirectToAction(nameof(Banners));
            }

            return null;
        }

        private void ValidateUserForm(UserFormViewModel model, bool requirePassword)
        {
            if (UserRoles.Options.All(option => option.Value != model.RoleName))
            {
                ModelState.AddModelError(nameof(model.RoleName), "Vai trò chưa đúng. Vui lòng chọn lại.");
            }

            if (requirePassword && string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Mật khẩu là bắt buộc.");
            }
        }

        private IActionResult? HandleUserWrite(AdminUserWriteResult result, bool redirectToList = false)
        {
            if (result.SessionExpired)
            {
                return ExpireSession();
            }

            if (redirectToList && !result.Success)
            {
                TempData["Error"] = result.ErrorMessage ?? UserFacingErrors.TryAgain;
                return RedirectToAction(nameof(Users));
            }

            return null;
        }

        private Guid GetCurrentUserId()
        {
            return Guid.TryParse(HttpContext.Session.GetString(AccountController.SessionUserId), out var id)
                ? id
                : Guid.Empty;
        }

        private IActionResult ExpireSession()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private static void SetStatusCounts(AdminNewsListViewModel model, List<NewsDto> news)
        {
            model.PostedCount = news.Count(item => NewsStatuses.IsPosted(item.Status));
            model.ArchivedCount = news.Count(item => item.Status == NewsStatuses.Archived);
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
