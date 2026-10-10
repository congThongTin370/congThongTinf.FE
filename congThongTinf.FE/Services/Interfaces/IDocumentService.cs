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
/// Danh sách tài liệu (public) và upload (Admin).
/// </summary>
public interface IDocumentService
{
    Task<List<DocumentDto>> GetDocumentsAsync(DocumentQuery query);
    Task<DocumentWriteResult> UploadAsync(string accessToken, DocumentUploadViewModel model);
}
