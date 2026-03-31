using System.Text.Json.Serialization;

namespace EasyID;

public sealed record IDCardVerify2Request(string Name, string IdNumber, string? TraceId = null);
public sealed record IDCardVerify3Request(string Name, string IdNumber, string Mobile, string? TraceId = null);
public sealed record IDCardVerifyResult(bool Result, bool Match, string Supplier, double Score, object? Raw = null);
public sealed record IDCardOCRResult(string Side, string Name, [property: JsonPropertyName("id_number")] string IdNumber, string? Gender = null, string? Nation = null, string? Birth = null, string? Address = null, string? Issue = null, string? Valid = null);
public sealed record PhoneStatusResult(string Status, string Carrier, string Province, bool Roaming);
public sealed record PhoneVerify3Request(string Name, string IdNumber, string Mobile);
public sealed record PhoneVerify3Result(bool Result, bool Match, string Supplier, double Score);
public sealed record LivenessResult(bool Liveness, double Score, string Method, [property: JsonPropertyName("frames_analyzed")] int FramesAnalyzed, [property: JsonPropertyName("attack_type")] string? AttackType);
public sealed record CompareResult(bool Match, double Score);
public sealed record FaceVerifyRequest(string IdNumber, string? MediaKey = null, string? CallbackUrl = null);
public sealed record FaceVerifyResult(bool Result, string Supplier, double Score);
public sealed record BankVerify4Request(string Name, string IdNumber, string BankCard, string? Mobile = null, string? TraceId = null);
public sealed record BankVerify4Result(bool Result, bool Match, [property: JsonPropertyName("bank_name")] string BankName, string Supplier, double Score, [property: JsonPropertyName("masked_bank_card")] string MaskedBankCard, [property: JsonPropertyName("card_type")] string CardType);
public sealed record RiskScoreRequest(string? Ip = null, string? DeviceFingerprint = null, string? DeviceId = null, string? Phone = null, string? Email = null, string? UserAgent = null, string? Action = null, int? Amount = null, Dictionary<string, object>? Context = null);
public sealed record RiskDetails([property: JsonPropertyName("rule_score")] int? RuleScore, [property: JsonPropertyName("ml_score")] int? MlScore);
public sealed record RiskScoreResult([property: JsonPropertyName("risk_score")] int RiskScore, string[] Reasons, string Recommendation, RiskDetails Details);
public sealed record StoreFingerprintRequest(string DeviceId, Dictionary<string, object> Fingerprint);
public sealed record StoreFingerprintResult([property: JsonPropertyName("device_id")] string DeviceId, bool Stored);
public sealed record BillingBalanceResult([property: JsonPropertyName("app_id")] string AppId, [property: JsonPropertyName("available_cents")] int AvailableCents);
public sealed record BillingRecord(int Id, [property: JsonPropertyName("app_id")] string AppId, [property: JsonPropertyName("request_id")] string RequestId, [property: JsonPropertyName("change_cents")] int ChangeCents, [property: JsonPropertyName("balance_before")] int BalanceBefore, [property: JsonPropertyName("balance_after")] int BalanceAfter, string Reason, string Operator, [property: JsonPropertyName("created_at")] int CreatedAt);
public sealed record BillingRecordsResult(int Total, int Page, BillingRecord[] Records);
