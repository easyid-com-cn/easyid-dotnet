namespace EasyID;

public sealed class IDCardApi
{
    private readonly Transport _transport;
    internal IDCardApi(Transport transport) => _transport = transport;
    public Task<IDCardVerifyResult> Verify2Async(IDCardVerify2Request request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<IDCardVerifyResult>(HttpMethod.Post, "/v1/idcard/verify2", null, new { name = request.Name, id_number = request.IdNumber, trace_id = request.TraceId }, cancellationToken);
    public Task<IDCardVerifyResult> Verify3Async(IDCardVerify3Request request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<IDCardVerifyResult>(HttpMethod.Post, "/v1/idcard/verify3", null, new { name = request.Name, id_number = request.IdNumber, mobile = request.Mobile, trace_id = request.TraceId }, cancellationToken);
    public Task<IDCardOCRResult> OcrAsync(string side, byte[] image, string filename = "image.bin", CancellationToken cancellationToken = default) => _transport.RequestMultipartAsync<IDCardOCRResult>("/v1/ocr/idcard", new Dictionary<string, string> { ["side"] = side }, [( "image", image, filename )], cancellationToken);
}

public sealed class PhoneApi
{
    private readonly Transport _transport;
    internal PhoneApi(Transport transport) => _transport = transport;
    public Task<PhoneStatusResult> StatusAsync(string phone, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<PhoneStatusResult>(HttpMethod.Get, "/v1/phone/status", new Dictionary<string, string> { ["phone"] = phone }, null, cancellationToken);
    public Task<PhoneVerify3Result> Verify3Async(PhoneVerify3Request request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<PhoneVerify3Result>(HttpMethod.Post, "/v1/phone/verify3", null, new { name = request.Name, id_number = request.IdNumber, mobile = request.Mobile }, cancellationToken);
}

public sealed class FaceApi
{
    private readonly Transport _transport;
    internal FaceApi(Transport transport) => _transport = transport;
    public Task<LivenessResult> LivenessAsync(byte[] media, string? mode = null, string filename = "media.bin", CancellationToken cancellationToken = default) => _transport.RequestMultipartAsync<LivenessResult>("/v1/face/liveness", mode is null ? new Dictionary<string, string>() : new Dictionary<string, string> { ["mode"] = mode }, [( "media", media, filename )], cancellationToken);
    public Task<CompareResult> CompareAsync(byte[] image1, byte[] image2, string filename1 = "image1.bin", string filename2 = "image2.bin", CancellationToken cancellationToken = default) => _transport.RequestMultipartAsync<CompareResult>("/v1/face/compare", new Dictionary<string, string>(), [( "image1", image1, filename1 ), ( "image2", image2, filename2 )], cancellationToken);
    public Task<FaceVerifyResult> VerifyAsync(FaceVerifyRequest request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<FaceVerifyResult>(HttpMethod.Post, "/v1/face/verify", null, new { id_number = request.IdNumber, media_key = request.MediaKey, callback_url = request.CallbackUrl }, cancellationToken);
}

public sealed class BankApi
{
    private readonly Transport _transport;
    internal BankApi(Transport transport) => _transport = transport;
    public Task<BankVerify4Result> Verify4Async(BankVerify4Request request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<BankVerify4Result>(HttpMethod.Post, "/v1/bank/verify4", null, new { name = request.Name, id_number = request.IdNumber, bank_card = request.BankCard, mobile = request.Mobile, trace_id = request.TraceId }, cancellationToken);
}

public sealed class RiskApi
{
    private readonly Transport _transport;
    internal RiskApi(Transport transport) => _transport = transport;
    public Task<RiskScoreResult> ScoreAsync(RiskScoreRequest request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<RiskScoreResult>(HttpMethod.Post, "/v1/risk/score", null, new { ip = request.Ip, device_fingerprint = request.DeviceFingerprint, device_id = request.DeviceId, phone = request.Phone, email = request.Email, user_agent = request.UserAgent, action = request.Action, amount = request.Amount, context = request.Context }, cancellationToken);
    public Task<StoreFingerprintResult> StoreFingerprintAsync(StoreFingerprintRequest request, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<StoreFingerprintResult>(HttpMethod.Post, "/v1/device/fingerprint", null, new { device_id = request.DeviceId, fingerprint = request.Fingerprint }, cancellationToken);
}

public sealed class BillingApi
{
    private readonly Transport _transport;
    internal BillingApi(Transport transport) => _transport = transport;
    public Task<BillingBalanceResult> BalanceAsync(string appId, CancellationToken cancellationToken = default) => _transport.RequestJsonAsync<BillingBalanceResult>(HttpMethod.Get, "/v1/billing/balance", new Dictionary<string, string> { ["app_id"] = appId }, null, cancellationToken);
    public Task<BillingRecordsResult> RecordsAsync(string appId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var normalizedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        return _transport.RequestJsonAsync<BillingRecordsResult>(HttpMethod.Get, "/v1/billing/records", new Dictionary<string, string> { ["app_id"] = appId, ["page"] = Math.Max(page, 1).ToString(), ["page_size"] = normalizedPageSize.ToString() }, null, cancellationToken);
    }
}
