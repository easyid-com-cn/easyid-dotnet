using System.Net;
using System.Text;
using Xunit;

namespace EasyID.Tests;

public sealed class EasyIDClientTests
{
    private const string KeyId = "ak_3f9a2b1c7d4e8f0a";
    private const string Secret = "sk_test";

    [Fact]
    public async Task CoversHappyPathsAndValidation()
    {
        var queue = new Queue<HttpResponseMessage>(new[]
        {
            Ok("{\"result\":true,\"match\":true,\"supplier\":\"aliyun\",\"score\":0.98}"),
            Ok("{\"result\":true,\"match\":true,\"supplier\":\"tencent\",\"score\":0.95}"),
            Ok("{\"side\":\"front\",\"name\":\"张三\",\"id_number\":\"110101199001011234\"}"),
            Ok("{\"status\":\"real\",\"carrier\":\"移动\",\"province\":\"广东\",\"roaming\":false}"),
            Ok("{\"result\":true,\"match\":true,\"supplier\":\"aliyun\",\"score\":0.99}"),
            Ok("{\"liveness\":true,\"score\":0.97,\"method\":\"passive\",\"frames_analyzed\":10,\"attack_type\":null}"),
            Ok("{\"match\":true,\"score\":0.92}"),
            Ok("{\"result\":true,\"supplier\":\"aliyun\",\"score\":0.96}"),
            Ok("{\"result\":true,\"match\":true,\"bank_name\":\"工商银行\",\"supplier\":\"aliyun\",\"score\":0.99,\"masked_bank_card\":\"6222****1234\",\"card_type\":\"debit\"}"),
            Ok("{\"risk_score\":30,\"reasons\":[\"new_device\"],\"recommendation\":\"allow\",\"details\":{\"rule_score\":null,\"ml_score\":null}}"),
            Ok("{\"device_id\":\"dev_abc\",\"stored\":true}"),
            Ok("{\"app_id\":\"app_001\",\"available_cents\":100000}"),
            Ok("{\"total\":1,\"page\":1,\"records\":[{\"id\":1,\"app_id\":\"app_001\",\"request_id\":\"req_001\",\"change_cents\":-100,\"balance_before\":100100,\"balance_after\":100000,\"reason\":\"idcard_verify2\",\"operator\":\"system\",\"created_at\":1711900000}]}")
        });
        var requests = new List<HttpRequestMessage>();
        var client = CreateClient(queue, requests);

        Assert.True((await client.IdCard.Verify2Async(new IDCardVerify2Request("张三", "110101199001011234"))).Result);
        Assert.True((await client.IdCard.Verify3Async(new IDCardVerify3Request("张三", "110101199001011234", "13800138000"))).Match);
        Assert.Equal("张三", (await client.IdCard.OcrAsync("front", Encoding.UTF8.GetBytes("image"), "id.jpg")).Name);
        Assert.Equal("real", (await client.Phone.StatusAsync("13800138000")).Status);
        Assert.True((await client.Phone.Verify3Async(new PhoneVerify3Request("张三", "110101199001011234", "13800138000"))).Result);
        Assert.True((await client.Face.LivenessAsync(Encoding.UTF8.GetBytes("video"), "passive")).Liveness);
        Assert.True((await client.Face.CompareAsync(Encoding.UTF8.GetBytes("img1"), Encoding.UTF8.GetBytes("img2"))).Match);
        Assert.True((await client.Face.VerifyAsync(new FaceVerifyRequest("110101199001011234", "oss://bucket/key"))).Result);
        Assert.Equal("debit", (await client.Bank.Verify4Async(new BankVerify4Request("张三", "110101199001011234", "6222021234567890", "13800138000"))).CardType);
        Assert.Equal(30, (await client.Risk.ScoreAsync(new RiskScoreRequest(Ip: "1.2.3.4", DeviceId: "dev_abc", Action: "login"))).RiskScore);
        Assert.True((await client.Risk.StoreFingerprintAsync(new StoreFingerprintRequest("dev_abc", new Dictionary<string, object> { ["canvas"] = "hash123" }))).Stored);
        Assert.Equal(100000, (await client.Billing.BalanceAsync("app_001")).AvailableCents);
        Assert.Equal(1, (await client.Billing.RecordsAsync("app_001")).Total);

        Assert.All(requests, request =>
        {
            Assert.Equal(KeyId, request.Headers.GetValues("X-Key-ID").Single());
            Assert.StartsWith("easyid-dotnet/", request.Headers.GetValues("User-Agent").Single());
            Assert.NotEmpty(request.Headers.GetValues("X-Signature").Single());
        });

        Assert.Throws<ArgumentException>(() => new EasyIDClient("sk_abc", Secret));
        Assert.Throws<ArgumentException>(() => new EasyIDClient(KeyId, ""));
    }

    [Fact]
    public async Task CoversApiErrorsAndHttpErrors()
    {
        var apiClient = CreateClient(new Queue<HttpResponseMessage>(new[] { Response(HttpStatusCode.OK, "{\"code\":1001,\"message\":\"invalid key_id\",\"request_id\":\"err-rid\",\"data\":null}") }), new List<HttpRequestMessage>());
        var apiError = await Assert.ThrowsAsync<ApiErrorException>(() => apiClient.Phone.StatusAsync("13800138000"));
        Assert.Equal(1001, apiError.Code);

        var httpClient = CreateClient(new Queue<HttpResponseMessage>(new[] { Response(HttpStatusCode.ServiceUnavailable, "<html>503</html>", "text/html") }), new List<HttpRequestMessage>());
        await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.Phone.StatusAsync("13800138000"));

        var jsonClient = CreateClient(new Queue<HttpResponseMessage>(new[] { Response(HttpStatusCode.InternalServerError, "{\"code\":5000,\"message\":\"internal server error\",\"request_id\":\"err-500\",\"data\":null}") }), new List<HttpRequestMessage>());
        var jsonError = await Assert.ThrowsAsync<ApiErrorException>(() => jsonClient.Phone.StatusAsync("13800138000"));
        Assert.Equal(5000, jsonError.Code);
    }

    private static EasyIDClient CreateClient(Queue<HttpResponseMessage> queue, List<HttpRequestMessage> requests) =>
        new(KeyId, Secret, options =>
        {
            options.BaseUrl = "https://api.easyid.test";
            options.HttpClient = new HttpClient(new QueueHandler(queue, requests));
        });

    private static HttpResponseMessage Ok(string data) => Response(HttpStatusCode.OK, $"{{\"code\":0,\"message\":\"success\",\"request_id\":\"test-rid\",\"data\":{data}}}");
    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body, string contentType = "application/json") => new(statusCode) { Content = new StringContent(body, Encoding.UTF8, contentType) };

    private sealed class QueueHandler(Queue<HttpResponseMessage> queue, List<HttpRequestMessage> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requests.Add(request);
            return Task.FromResult(queue.Dequeue());
        }
    }
}
