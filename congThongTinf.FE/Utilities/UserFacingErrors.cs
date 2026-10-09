namespace congThongTinf.FE.Utilities;

/// <summary>
/// Đổi mã lỗi và câu của API thành câu tiếng Việt cho người dùng không kỹ thuật.
/// Không trả mã HTTP, Guid, hay chi tiết exception ra giao diện.
/// </summary>
public static class UserFacingErrors
{
    public const string TryAgain = "Chưa thực hiện được. Vui lòng thử lại sau.";
    public const string SessionExpired = "Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.";
    public const string NoPermission = "Bạn không có quyền thực hiện thao tác này.";
    public const string LoadNewsFailed = "Chưa tải được tin tức. Vui lòng tải lại trang.";

    public static string ForLogin(string? code, string? message)
    {
        var text = message ?? string.Empty;

        if (Contains(text, "khóa", "cấm") || code == "403")
        {
            return "Tài khoản đang bị khóa. Vui lòng liên hệ quản trị viên.";
        }

        if (Contains(text, "vai trò", "quyền"))
        {
            return "Tài khoản chưa được cấp quyền sử dụng. Vui lòng liên hệ quản trị viên.";
        }

        if (Contains(text, "mật khẩu") || code == "401")
        {
            return "Mật khẩu chưa đúng. Vui lòng nhập lại.";
        }

        if (Contains(text, "email", "tài khoản") || code == "404")
        {
            return "Không tìm thấy tài khoản với email này.";
        }

        if (code == "500")
        {
            return "Hệ thống đang bận. Vui lòng thử lại sau.";
        }

        return "Không đăng nhập được. Vui lòng kiểm tra email, mật khẩu và thử lại.";
    }

    public static string ForNews(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (Contains(text, "chính mình"))
        {
            return "Bạn chỉ được sửa hoặc xóa bài viết của mình.";
        }

        if (Contains(text, "phóng viên"))
        {
            return "Chỉ phóng viên mới được viết và sửa bài.";
        }

        if (Contains(text, "người dùng") || status == "401")
        {
            return SessionExpired;
        }

        if (Contains(text, "danh mục") && (status == "404" || Contains(text, "không tồn tại", "không tìm thấy")))
        {
            return "Không tìm thấy chuyên mục đã chọn. Vui lòng chọn chuyên mục khác.";
        }

        if (status == "404" || Contains(text, "không tồn tại", "không tìm thấy"))
        {
            return "Không tìm thấy bài viết này.";
        }

        if (Contains(text, "bắt buộc", "tiêu đề", "nội dung"))
        {
            return "Vui lòng nhập đủ tiêu đề, nội dung và chuyên mục.";
        }

        if (status == "400")
        {
            return "Thông tin bài viết chưa đúng. Vui lòng kiểm tra lại.";
        }

        if (status == "403")
        {
            return NoPermission;
        }

        if (status == "409")
        {
            return "Không thực hiện được vì dữ liệu đang bị trùng hoặc còn liên quan.";
        }

        return TryAgain;
    }

    private static bool Contains(string text, params string[] parts)
    {
        foreach (var part in parts)
        {
            if (text.Contains(part, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
