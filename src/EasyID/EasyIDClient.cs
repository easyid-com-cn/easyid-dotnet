namespace EasyID;

public sealed class EasyIDClient
{
    private static readonly System.Text.RegularExpressions.Regex KeyIdRegex = new("^ak_[0-9a-f]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public IDCardApi IdCard { get; }
    public PhoneApi Phone { get; }
    public FaceApi Face { get; }
    public BankApi Bank { get; }
    public RiskApi Risk { get; }
    public BillingApi Billing { get; }

    public EasyIDClient(string keyId, string secret, Action<EasyIDOptions>? configure = null)
    {
        if (!KeyIdRegex.IsMatch(keyId))
        {
            throw new ArgumentException($"easyid: keyId must match ak_<hex>, got: {keyId}", nameof(keyId));
        }
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("easyid: secret must not be empty", nameof(secret));
        }

        var options = new EasyIDOptions();
        configure?.Invoke(options);
        var transport = new Transport(keyId, secret, options.BaseUrl, options.HttpClient);
        IdCard = new IDCardApi(transport);
        Phone = new PhoneApi(transport);
        Face = new FaceApi(transport);
        Bank = new BankApi(transport);
        Risk = new RiskApi(transport);
        Billing = new BillingApi(transport);
    }
}

public sealed class EasyIDOptions
{
    public string BaseUrl { get; set; } = Transport.DefaultBaseUrl;
    public HttpClient HttpClient { get; set; } = new();
}
