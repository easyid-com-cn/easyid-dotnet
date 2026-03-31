using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasyID;

internal sealed class Transport
{
    public const string DefaultBaseUrl = "https://api.easyid.com";
    public const string Version = "1.0.0";
    private const int MaxResponseBytes = 10 << 20;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _keyId;
    private readonly string _secret;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public Transport(string keyId, string secret, string baseUrl, HttpClient httpClient)
    {
        _keyId = keyId;
        _secret = secret;
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public Task<T> RequestJsonAsync<T>(HttpMethod method, string path, IReadOnlyDictionary<string, string>? query, object? body, CancellationToken cancellationToken = default)
    {
        var bodyBytes = body is null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
        return SendAsync<T>(method, path, query, bodyBytes, "application/json", cancellationToken);
    }

    public async Task<T> RequestMultipartAsync<T>(string path, IReadOnlyDictionary<string, string> fields, IReadOnlyList<(string Name, byte[] Content, string FileName)> files, CancellationToken cancellationToken = default)
    {
        var boundary = "----easyid-" + Guid.NewGuid();
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        foreach (var field in fields)
        {
            await writer.WriteAsync($"--{boundary}\r\nContent-Disposition: form-data; name=\"{field.Key}\"\r\n\r\n{field.Value}\r\n");
        }
        await writer.FlushAsync();
        foreach (var file in files)
        {
            await writer.WriteAsync($"--{boundary}\r\nContent-Disposition: form-data; name=\"{file.Name}\"; filename=\"{file.FileName}\"\r\nContent-Type: application/octet-stream\r\n\r\n");
            await writer.FlushAsync();
            await stream.WriteAsync(file.Content, cancellationToken);
            await writer.WriteAsync("\r\n");
            await writer.FlushAsync();
        }
        await writer.WriteAsync($"--{boundary}--\r\n");
        await writer.FlushAsync();
        return await SendAsync<T>(HttpMethod.Post, path, null, stream.ToArray(), $"multipart/form-data; boundary={boundary}", cancellationToken);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, IReadOnlyDictionary<string, string>? query, byte[] body, string contentType, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = Signer.Sign(_secret, timestamp, query, body);
        var uriBuilder = new UriBuilder(_baseUrl + path);
        if (query is not null && query.Count > 0)
        {
            uriBuilder.Query = string.Join("&", query.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        }

        using var request = new HttpRequestMessage(method, uriBuilder.Uri);
        request.Headers.TryAddWithoutValidation("X-Key-ID", _keyId);
        request.Headers.TryAddWithoutValidation("X-Timestamp", timestamp);
        request.Headers.TryAddWithoutValidation("X-Signature", signature);
        request.Headers.TryAddWithoutValidation("User-Agent", $"easyid-dotnet/{Version}");
        if (body.Length > 0)
        {
            request.Content = new ByteArrayContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }
        else if (method != HttpMethod.Get)
        {
            request.Content = new ByteArrayContent(Array.Empty<byte>());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (raw.Length > MaxResponseBytes)
        {
            throw new InvalidOperationException("easyid: response exceeds 10 MB limit");
        }

        if (!response.IsSuccessStatusCode)
        {
            var apiError = TryParseApiError(raw);
            if (apiError is not null)
            {
                throw apiError;
            }
            throw new HttpRequestException($"easyid: http status {(int)response.StatusCode}");
        }

        var envelope = JsonSerializer.Deserialize<Envelope>(raw, JsonOptions) ?? throw new InvalidOperationException("easyid: empty response");
        if (envelope.Code != 0)
        {
            throw new ApiErrorException(envelope.Code, envelope.Message ?? string.Empty, envelope.RequestId ?? string.Empty);
        }
        return envelope.Data.Deserialize<T>(JsonOptions)!;
    }

    private static ApiErrorException? TryParseApiError(byte[] raw)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(raw, JsonOptions);
            return envelope is not null && envelope.Code != 0
                ? new ApiErrorException(envelope.Code, envelope.Message ?? string.Empty, envelope.RequestId ?? string.Empty)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private sealed class Envelope
    {
        public int Code { get; init; }
        public string? Message { get; init; }
        public string? RequestId { get; init; }
        public JsonNode Data { get; init; } = JsonValue.Create((string?) null)!;
    }
}
