# EasyID .NET SDK

Official .NET SDK for the EasyID identity verification API.

## Install

```bash
dotnet add package EasyID
```

## Quick Start

```csharp
var client = new EasyIDClient("ak_xxx", "sk_xxx");

try
{
    var result = await client.IdCard.Verify2Async(new IDCardVerify2Request("张三", "110101199001011234"));
    Console.WriteLine(result.Match);
}
catch (ApiErrorException error)
{
    Console.WriteLine($"{error.Code} {error.RequestId}");
}
```

## Supported APIs

- IDCard: `Verify2Async`, `Verify3Async`, `OcrAsync`
- Phone: `StatusAsync`, `Verify3Async`
- Face: `LivenessAsync`, `CompareAsync`, `VerifyAsync`
- Bank: `Verify4Async`
- Risk: `ScoreAsync`, `StoreFingerprintAsync`
- Billing: `BalanceAsync`, `RecordsAsync`

## Configuration

- `options.BaseUrl`
- `options.HttpClient`

## Security Notice

This is a server-side SDK. Do not expose `secret` in browsers or mobile apps.

## More Docs

- [Integration Guide](/Users/nbt-mingyi/mingyi.wu/easyid/sdk/docs/integration-guide.md)
- [Publishing Strategy](/Users/nbt-mingyi/mingyi.wu/easyid/sdk/docs/repository-publishing-strategy.md)
