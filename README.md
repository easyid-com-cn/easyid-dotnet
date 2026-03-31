# EasyID .NET SDK

Official .NET SDK for the EasyID identity verification API.

## Install

```bash
dotnet add package EasyID
```

## Usage

```csharp
var client = new EasyIDClient("ak_xxx", "sk_xxx");
var result = await client.IdCard.Verify2Async(new IDCardVerify2Request("张三", "110101199001011234"));

Console.WriteLine(result.Match);
```

## Notes

- This is a server-side SDK. Do not expose `secret` in browsers or mobile apps.
- Configure `BaseUrl` for private deployments.
- See `../docs/integration-guide.md` for end-to-end integration and troubleshooting.
