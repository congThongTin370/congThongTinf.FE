using System.ComponentModel.DataAnnotations;
using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Models;

public class AdminUserListViewModel
{
    public List<UserDto> Items { get; set; } = new();
    public string? Search { get; set; }
    public int? RoleName { get; set; }
    public bool? IsActive { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool HasPrevious => PageIndex > 1;
    public bool HasNext => Items.Count >= PageSize;
    public string? LoadError { get; set; }
    public Guid CurrentUserId { get; set; }
}

public class UserFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email chưa đúng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Vai trò")]
    public int RoleName { get; set; } = UserRoles.Journalist;

    [Display(Name = "Đang hoạt động")]
    public bool IsActive { get; set; } = true;

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;
}

public class AdminUserWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public UserDto? Data { get; set; }
}

public class AdminUserListResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public List<UserDto> Items { get; set; } = new();
}

public class AdminUserQuery
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int? RoleName { get; set; }
}

public static class UserRoles
{
    public const int Admin = 0;
    public const int Journalist = 1;

    public static readonly (int Value, string Label)[] Options =
    {
        (Admin, "Quản trị viên"),
        (Journalist, "Phóng viên")
    };

    public static string Label(int roleName)
    {
        foreach (var option in Options)
        {
            if (option.Value == roleName)
            {
                return option.Label;
            }
        }

        return "Không xác định";
    }
}
