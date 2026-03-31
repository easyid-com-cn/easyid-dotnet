namespace EasyID;

public sealed class ApiErrorException : Exception
{
    public int Code { get; }
    public string RequestId { get; }

    public ApiErrorException(int code, string message, string requestId)
        : base($"easyid: code={code} message={message} request_id={requestId}")
    {
        Code = code;
        RequestId = requestId;
    }
}
