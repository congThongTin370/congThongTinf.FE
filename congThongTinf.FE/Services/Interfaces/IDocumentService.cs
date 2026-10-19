using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public class DocumentQuery
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? Category { get; set; }
}

/// <summary>
/// Danh sách tài liệu (public), upload và xóa (Admin).
/// </summary>
public interface IDocumentService
{
    Task<List<DocumentDto>> GetDocumentsAsync(DocumentQuery query);
    Task<DocumentWriteResult> UploadAsync(string accessToken, DocumentUploadViewModel model);
    Task<DocumentWriteResult> DeleteAsync(string accessToken, Guid id);

    /// <summary>
    /// Lấy bytes file từ GET /api/documents/{id}/download để FE stream lại cho trình duyệt.
    /// </summary>
    Task<SharedFileStreamResult> OpenFileAsync(Guid id);
}
