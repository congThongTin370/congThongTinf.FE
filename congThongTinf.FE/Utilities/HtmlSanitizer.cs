using System.Net;
using System.Text.RegularExpressions;
using Markdig;

namespace congThongTinf.FE.Utilities;

/// <summary>
/// Làm sạch HTML của nội dung bài báo trước khi hiển thị.
/// Chỉ giữ thẻ định dạng an toàn và một số thuộc tính được kiểm soát (href/src http(s), class căn lề, style màu/cỡ chữ).
/// Mọi thẻ/thuộc tính khác (script, iframe, on*, ...) bị loại bỏ.
/// </summary>
public static partial class HtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "s", "span",
        "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "pre", "code",
        "ul", "ol", "li", "a", "img"
    };

    private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase) { "br", "img" };

    [GeneratedRegex(@"<\s*(script|style)[^>]*>[\s\S]*?<\s*/\s*\1\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex DangerousBlockRegex();

    [GeneratedRegex(@"<\s*(/?)\s*([a-zA-Z][a-zA-Z0-9]*)([^>]*)>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"([a-zA-Z\-]+)\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s""'>]+))")]
    private static partial Regex AttributeRegex();

    [GeneratedRegex(@"^ql-align-(left|right|center|justify)$")]
    private static partial Regex AlignClassRegex();

    [GeneratedRegex(@"^ql-size-(small|large|huge)$")]
    private static partial Regex SizeClassRegex();

    [GeneratedRegex(@"^(?:color|background-color|font-size)\s*:\s*[#a-zA-Z0-9(),.\s%-]+$", RegexOptions.IgnoreCase)]
    private static partial Regex SafeStyleRegex();

    /// <summary>
    /// Chuyển nội dung Markdown thành HTML rồi làm sạch.
    /// Raw HTML trong Markdown bị vô hiệu hóa; mọi output đều đi qua <see cref="Sanitize"/>.
    /// </summary>
    public static string RenderMarkdown(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml()
            .Build();

        var html = Markdown.ToHtml(markdown, pipeline);
        return Sanitize(html);
    }

    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // Bài cũ là văn bản thuần (không có thẻ): tách theo dòng thành đoạn <p>
        if (!html.Contains('<'))
        {
            var paragraphs = html.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => $"<p>{WebUtility.HtmlEncode(line.Trim())}</p>");
            return string.Concat(paragraphs);
        }

        // Xóa hẳn khối script/style, kể cả nội dung bên trong
        var text = DangerousBlockRegex().Replace(html, string.Empty);

        return TagRegex().Replace(text, match =>
        {
            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value.ToLowerInvariant();

            if (!AllowedTags.Contains(tagName))
            {
                return string.Empty;
            }

            if (isClosing)
            {
                return VoidTags.Contains(tagName) ? string.Empty : $"</{tagName}>";
            }

            var attributes = BuildAttributes(tagName, match.Groups[3].Value);
            var selfClose = VoidTags.Contains(tagName) ? " />" : ">";
            return $"<{tagName}{attributes}{selfClose}";
        });
    }

    /// <summary>
    /// Chỉ giữ các thuộc tính được phép cho từng thẻ, đã kiểm tra giá trị.
    /// </summary>
    private static string BuildAttributes(string tagName, string rawAttributes)
    {
        var parts = new List<string>();

        foreach (Match attr in AttributeRegex().Matches(rawAttributes))
        {
            var name = attr.Groups[1].Value.ToLowerInvariant();
            var value = attr.Groups[2].Success ? attr.Groups[2].Value
                : attr.Groups[3].Success ? attr.Groups[3].Value
                : attr.Groups[4].Value;

            switch (name)
            {
                case "href" when tagName == "a" && IsSafeUrl(value):
                    parts.Add($"href=\"{WebUtility.HtmlEncode(value)}\"");
                    parts.Add("rel=\"nofollow noopener\"");
                    parts.Add("target=\"_blank\"");
                    break;

                case "src" when tagName == "img" && IsSafeUrl(value):
                    parts.Add($"src=\"{WebUtility.HtmlEncode(value)}\"");
                    parts.Add("loading=\"lazy\"");
                    break;

                case "alt" when tagName == "img":
                    parts.Add($"alt=\"{WebUtility.HtmlEncode(value)}\"");
                    break;

                case "class" when tagName is "p" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "li" or "span":
                    // Chỉ giữ class căn lề / cỡ chữ do trình soạn thảo sinh ra
                    var allowedClass = value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault(c => AlignClassRegex().IsMatch(c) || SizeClassRegex().IsMatch(c));
                    if (allowedClass != null)
                    {
                        parts.Add($"class=\"{allowedClass}\"");
                    }
                    break;

                case "style" when tagName == "span":
                    var safeStyles = value.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => SafeStyleRegex().IsMatch(s));
                    var joined = string.Join("; ", safeStyles);
                    if (joined.Length > 0)
                    {
                        parts.Add($"style=\"{WebUtility.HtmlEncode(joined)}\"");
                    }
                    break;
            }
        }

        return parts.Count == 0 ? string.Empty : " " + string.Join(" ", parts);
    }

    private static bool IsSafeUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase));
    }
}
