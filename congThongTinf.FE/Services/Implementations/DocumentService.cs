using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class DocumentService : IDocumentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _publicBaseUrl;

    public DocumentService(HttpClient httpClient, IConfiguration configuration, ILogger<DocumentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _publicBaseUrl = ResolvePublicBaseUrl(configuration);
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync(DocumentQuery query)
    {
        try
        {
            var url = BuildListUrl(query);
            using var response = await _httpClient.GetAsync(url);
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<PagedData<DocumentDto>>>();

            if (body?.IsSuccess != true || body.Value?.Data == null)
            {
                return new List<DocumentDto>();
            }

            foreach (var document in body.Value.Data.Items)
            {
                SetBrowserDownloadUrl(document);
            }

            return body.Value.Data.Items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load documents");
            return new List<DocumentDto>();
        }
    }

    public async Task<DocumentWriteResult> UploadAsync(string accessToken, DocumentUploadViewModel model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            return Failure(400, "400", "File là bắt buộc.");
        }

        try
        {
            using var content = new MultipartFormDataContent();
            await using var fileStream = model.File.OpenReadStream();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(model.File.ContentType)
                    ? "application/octet-stream"
                    : model.File.ContentType);
            content.Add(fileContent, "file", model.File.FileName);

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                content.Add(new StringContent(model.Title.Trim()), "title");
            }

            content.Add(new StringContent(model.Description?.Trim() ?? string.Empty), "description");
            content.Add(new StringContent(model.Category.ToString()), "category");

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/documents")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new DocumentWriteResult
                {
                    Success = false,
                    SessionExpired = true,
                    ErrorMessage = UserFacingErrors.SessionExpired
                };
            }

            BackendEnvelope<DocumentDto>? body = null;
            var responseText = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                body = System.Text.Json.JsonSerializer.Deserialize<BackendEnvelope<DocumentDto>>(
                    responseText,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
            {
                return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            SetBrowserDownloadUrl(body.Value.Data);
            return new DocumentWriteResult { Success = true, Data = body.Value.Data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document");
            return Failure(0, null, null);
        }
    }

    public async Task<DocumentWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/documents/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new DocumentWriteResult
                {
                    Success = false,
                    SessionExpired = true,
                    ErrorMessage = UserFacingErrors.SessionExpired
                };
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new DocumentWriteResult
                {
                    Success = false,
                    ErrorMessage = "Tài liệu không còn tồn tại hoặc đã bị xóa."
                };
            }

            BackendEnvelope<bool>? body = null;
            var responseText = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                body = System.Text.Json.JsonSerializer.Deserialize<BackendEnvelope<bool>>(
                    responseText,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            return new DocumentWriteResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {DocumentId}", id);
            return Failure(0, null, null);
        }
    }

    private static string BuildListUrl(DocumentQuery query)
    {
        var parts = new List<string>
        {
            $"pageIndex={query.PageIndex}",
            $"pageSize={query.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            parts.Add($"search={Uri.EscapeDataString(query.Search.Trim())}");
        }

        if (query.Category.HasValue)
        {
            parts.Add($"category={query.Category.Value}");
        }

        return "api/documents?" + string.Join("&", parts);
    }

    private void SetBrowserDownloadUrl(DocumentDto document)
    {
        document.DownloadUrl = $"{_publicBaseUrl}/api/documents/{document.Id}/download";
    }

    private static string ResolvePublicBaseUrl(IConfiguration configuration)
    {
        var publicBase = configuration["ApiSettings:PublicBaseUrl"];
        var serverBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        var baseUrl = string.IsNullOrWhiteSpace(publicBase) ? serverBase : publicBase;
        baseUrl = baseUrl.TrimEnd('/');

        // Container gọi API qua host.docker.internal; trình duyệt trên máy host dùng localhost.
        if (baseUrl.Contains("host.docker.internal", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = baseUrl.Replace("host.docker.internal", "localhost", StringComparison.OrdinalIgnoreCase);
        }

        return baseUrl;
    }

    private static DocumentWriteResult Failure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = UserFacingErrors.ForDocuments(statusCode, code, message)
    };
}
