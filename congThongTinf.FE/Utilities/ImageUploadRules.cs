namespace congThongTinf.FE.Utilities;

/// <summary>
/// Khớp GET contract: ảnh nhận image/* hoặc đuôi trong tài liệu backend, tối đa 10 MB.
/// </summary>
public static class ImageUploadRules
{
    public const long MaxBytes = 10 * 1024 * 1024;

    public static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".jfif", ".png", ".gif", ".webp", ".bmp",
        ".tif", ".tiff", ".tga", ".pbm", ".pgm", ".ppm", ".qoi"
    };

    public static bool IsAllowed(IFormFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.ContentType)
            && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrWhiteSpace(extension) && Extensions.Contains(extension);
    }
}
