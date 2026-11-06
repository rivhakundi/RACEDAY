using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Services;

public sealed class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    public RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor) { _http = http; _accessor = accessor; }
    private ISession? Session => _accessor.HttpContext?.Session;
    private HttpRequestMessage Request(HttpMethod method, string path, object? body = null)
    {
        var req = new HttpRequestMessage(method, path);
        var cookie = Session?.GetString("ApiCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) req.Headers.TryAddWithoutValidation("Cookie", cookie);
        if (body != null) req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return req;
    }
    public async Task<(bool Ok, string Message, JsonElement Data)> SendAsync(HttpMethod method, string path, object? body = null)
    {
        try
        {
            using var req = Request(method, path, body);
            using var res = await _http.SendAsync(req);
            if ((method == HttpMethod.Post && path.EndsWith("/login", StringComparison.OrdinalIgnoreCase)) || path.EndsWith("/register", StringComparison.OrdinalIgnoreCase))
            {
                if (res.Headers.TryGetValues("Set-Cookie", out var values))
                {
                    var cookie = string.Join("; ", values.Select(v => v.Split(';')[0]));
                    if (!string.IsNullOrWhiteSpace(cookie)) Session?.SetString("ApiCookie", cookie);
                }
            }
            var text = await res.Content.ReadAsStringAsync();
            JsonElement data = default;
            try { using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text); data = doc.RootElement.Clone(); } catch { }
            var message = res.IsSuccessStatusCode ? "Success" : ExtractMessage(text, res.StatusCode);
            return (res.IsSuccessStatusCode, message, data);
        }
        catch (TaskCanceledException) { return (false, "The RaceDay API timed out. Please try again.", default); }
        catch (HttpRequestException) { return (false, "Cannot connect to the RaceDay API. Check that the Part 2 API is running.", default); }
    }
    public async Task<T?> GetAsync<T>(string path)
    {
        var r = await SendAsync(HttpMethod.Get, path);
        if (!r.Ok || r.Data.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return default;
        try { return r.Data.Deserialize<T>(JsonOptions); } catch { return default; }
    }
    public async Task<(bool Ok, string Message)> UploadAsync(string path, IFormFile file, string fieldName = "file")
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, path);
            var cookie = Session?.GetString("ApiCookie"); if (!string.IsNullOrWhiteSpace(cookie)) req.Headers.TryAddWithoutValidation("Cookie", cookie);
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream(); var fileContent = new StreamContent(stream); fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(fileContent, fieldName, Path.GetFileName(file.FileName)); req.Content = content;
            using var response = await _http.SendAsync(req); var body = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Uploaded" : ExtractMessage(body, response.StatusCode));
        }
        catch (HttpRequestException) { return (false, "Cannot connect to the RaceDay API."); }
    }
    private static string ExtractMessage(string body, HttpStatusCode status)
    {
        if (!string.IsNullOrWhiteSpace(body)) { try { using var d = JsonDocument.Parse(body); if (d.RootElement.ValueKind == JsonValueKind.String) return d.RootElement.GetString() ?? "Request failed."; if (d.RootElement.TryGetProperty("message", out var m)) return m.GetString() ?? "Request failed."; if (d.RootElement.TryGetProperty("title", out var t)) return t.GetString() ?? "Request failed."; } catch { if (body.Length < 240 && !body.TrimStart().StartsWith('<')) return body; } }
        return $"The request could not be completed ({(int)status}). Please check your details and try again.";
    }
}
