# EasyID .NET SDK

EasyID .NET SDK 是易验云身份验证 API 的官方 .NET 客户端。

English README: [README.md](README.md)

EasyID 提供身份证核验、手机号核验、人脸识别、银行卡核验、风控评分等能力。本 SDK 适用于服务端 .NET 应用，自动处理签名、认证头和服务端业务错误。

## 安装

```bash
dotnet add package EasyID
```

要求：

- .NET 8+

## 快速开始

```csharp
using EasyID;

var client = new EasyIDClient("ak_xxx", "sk_xxx");

try
{
    var result = await client.IdCard.Verify2Async(new IDCardVerify2Request("张三", "110101199001011234"));
    Console.WriteLine($"是否匹配：{result.Match}");
}
catch (APIErrorException error)
{
    Console.WriteLine($"{error.Code} {error.Message} {error.RequestId}");
}
```

## 已支持接口

- `client.IdCard.Verify2Async(...)`：身份证二要素核验
- `client.IdCard.Verify3Async(...)`：身份证三要素核验
- `client.IdCard.OcrAsync(...)`：身份证 OCR
- `client.Phone.StatusAsync(...)`：手机号状态查询
- `client.Phone.Verify3Async(...)`：手机号三要素核验
- `client.Face.LivenessAsync(...)`：人脸活体检测
- `client.Face.CompareAsync(...)`：人脸比对
- `client.Face.VerifyAsync(...)`：人脸核验
- `client.Bank.Verify4Async(...)`：银行卡四要素核验
- `client.Risk.ScoreAsync(...)`：风控评分
- `client.Risk.StoreFingerprintAsync(...)`：存储设备指纹
- `client.Billing.BalanceAsync(...)`：查询账户余额
- `client.Billing.RecordsAsync(...)`：查询账单记录

## 配置项

- `options.BaseUrl`：自定义 API 地址
- `options.HttpClient`：自定义 `HttpClient`

## 错误处理

服务端业务错误会抛出 `APIErrorException`。

```csharp
try
{
    var result = await client.Phone.StatusAsync("13800138000");
    Console.WriteLine(result.Status);
}
catch (APIErrorException error)
{
    Console.WriteLine($"{error.Code} {error.Message} {error.RequestId}");
}
```

## 安全说明

- 这是服务端 SDK，不要在浏览器、桌面前端或移动端直接暴露 `secret`
- `keyId` 必须符合 `ak_[0-9a-f]+`
- SDK 会自动生成并附加签名请求头

## 官方资源

- 官网：`https://www.easyid.com.cn/`
- GitHub：`https://github.com/easyid-com-cn/`
