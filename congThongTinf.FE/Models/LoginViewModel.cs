using System.ComponentModel.DataAnnotations;

namespace congThongTinf.FE.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nhập tên đăng nhập.")]
        [StringLength(100)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập mật khẩu.")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
