using System.ComponentModel.DataAnnotations;

namespace congThongTinf.FE.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nhập email đăng nhập.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập mật khẩu.")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
