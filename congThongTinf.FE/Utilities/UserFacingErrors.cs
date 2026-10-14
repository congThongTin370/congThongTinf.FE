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

        if (Contains(text, "10 MB", "exceeds"))
        {
            return "Ảnh bìa không được lớn hơn 10 MB.";
        }

        if (Contains(text, "phải là", "jpg", "png", "webp", "gif") && Contains(text, "ảnh", "file"))
        {
            return "Ảnh bìa phải là jpg, jpeg, png, webp hoặc gif.";
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

    public static string ForNewsImage(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (status == "401")
        {
            return SessionExpired;
        }

        if (status == "403")
        {
            return NoPermission;
        }

        if (Contains(text, "10 MB", "exceeds"))
        {
            return "Ảnh không được lớn hơn 10 MB.";
        }

        if (Contains(text, "phải là", "jpg", "png", "webp", "gif"))
        {
            return "Chỉ nhận ảnh jpg, jpeg, png, webp hoặc gif.";
        }

        if (Contains(text, "bắt buộc") || status == "400")
        {
            return "Vui lòng chọn ảnh hợp lệ.";
        }

        return TryAgain;
    }

    public static string ForCategories(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (status == "401")
        {
            return SessionExpired;
        }

        if (status == "403")
        {
            return NoPermission;
        }

        if (Contains(text, "bài báo", "liên quan"))
        {
            return "Không thể xóa danh mục đang có bài báo.";
        }

        if (Contains(text, "đã tồn tại") || (status == "409" && Contains(text, "tên")))
        {
            return "Tên danh mục đã được dùng. Vui lòng chọn tên khác.";
        }

        if (status == "409")
        {
            return "Không thực hiện được vì danh mục đang được dùng hoặc tên bị trùng.";
        }

        if (Contains(text, "bắt buộc") || (status == "400" && Contains(text, "tên")))
        {
            return "Vui lòng nhập tên danh mục.";
        }

        if (status == "400")
        {
            return "Thông tin danh mục chưa đúng. Vui lòng kiểm tra lại.";
        }

        if (status == "404")
        {
            return "Không tìm thấy danh mục này.";
        }

        return TryAgain;
    }

    public static string ForUsers(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (status == "401")
        {
            return SessionExpired;
        }

        if (status == "403")
        {
            return NoPermission;
        }

        if (Contains(text, "đang đăng nhập", "chính mình"))
        {
            return "Không thể xóa tài khoản đang đăng nhập.";
        }

        if (Contains(text, "email") && (status == "409" || Contains(text, "đăng ký", "tồn tại")))
        {
            return "Email này đã được đăng ký. Vui lòng dùng email khác.";
        }

        if (Contains(text, "tên đăng nhập", "username") || (status == "409" && Contains(text, "đã được sử dụng")))
        {
            return "Tên đăng nhập đã được sử dụng. Vui lòng chọn tên khác.";
        }

        if (status == "409")
        {
            return "Email hoặc tên đăng nhập đã được dùng. Vui lòng chọn thông tin khác.";
        }

        if (Contains(text, "vai trò không hợp lệ"))
        {
            return "Vai trò chưa đúng. Vui lòng chọn lại.";
        }

        if (Contains(text, "seed", "chưa có vai trò"))
        {
            return "Hệ thống chưa sẵn sàng tạo tài khoản. Vui lòng liên hệ kỹ thuật.";
        }

        if (Contains(text, "bắt buộc") || (status == "400" && Contains(text, "username", "email", "mật khẩu", "họ tên")))
        {
            return "Vui lòng nhập đủ tên đăng nhập, email, mật khẩu và họ tên.";
        }

        if (status == "400")
        {
            return "Thông tin tài khoản chưa đúng. Vui lòng kiểm tra lại.";
        }

        if (status == "404")
        {
            return "Không tìm thấy người dùng này.";
        }

        return TryAgain;
    }

    public static string ForDocuments(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (status == "401")
        {
            return SessionExpired;
        }

        if (status == "403")
        {
            return NoPermission;
        }

        if (Contains(text, "10 MB", "10MB", "exceeds"))
        {
            return "Tệp không được lớn hơn 10 MB.";
        }

        if (Contains(text, "pdf", "docx", "xlsx", "pptx", "loại file", "phải là"))
        {
            return "Chỉ nhận tệp pdf, doc, docx, xls, xlsx, ppt, pptx hoặc txt.";
        }

        if (Contains(text, "danh mục"))
        {
            return "Danh mục tài liệu chưa đúng. Vui lòng chọn lại.";
        }

        if (Contains(text, "bắt buộc", "file"))
        {
            return "Vui lòng chọn tệp tài liệu.";
        }

        if (status == "400")
        {
            return "Thông tin tài liệu chưa đúng. Vui lòng kiểm tra lại.";
        }

        if (status == "404")
        {
            return "Không xác định được tài khoản. Vui lòng đăng nhập lại.";
        }

        return TryAgain;
    }

    public static string ForBanners(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (status == "401")
        {
            return SessionExpired;
        }

        if (status == "403")
        {
            return NoPermission;
        }

        if (Contains(text, "10 MB", "10MB", "exceeds"))
        {
            return "Ảnh banner không được lớn hơn 10 MB.";
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            return text.Trim();
        }

        if (status == "400")
        {
            return "Thông tin banner chưa đúng. Vui lòng kiểm tra lại.";
        }

        if (status == "404" && Contains(text, "banner"))
        {
            return "Không tìm thấy banner này.";
        }

        if (status == "404")
        {
            return "Không xác định được tài khoản. Vui lòng đăng nhập lại.";
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
