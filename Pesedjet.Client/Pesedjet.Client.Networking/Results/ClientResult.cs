using System.Diagnostics;

namespace Pesedjet.Client.Networking.Results;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ClientResult<TValue>
{
    private ClientResult()
    {
    }

    public bool IsSuccess { get; private set; }

    public TValue? Value { get; private set; }

    public FailureKind Failure { get; private set; }
    
    public string ErrorCode { get; private set; } = string.Empty;

    public string MessageKey { get; private set; } = string.Empty;

    public bool IsConnectionFailure => Failure is FailureKind.ServerUnreachable or FailureKind.Timeout;

    public static ClientResult<TValue> Success(TValue value)
    {
        return new ClientResult<TValue> { IsSuccess = true, Value = value, Failure = FailureKind.None };
    }

    public static ClientResult<TValue> BusinessFailure(string errorCode, string messageKey)
    {
        return new ClientResult<TValue>
        {
            Failure = FailureKind.Business,
            ErrorCode = errorCode,
            MessageKey = messageKey
        };
    }

    public static ClientResult<TValue> TechnicalFailure(FailureKind failure)
    {
        return new ClientResult<TValue> { Failure = failure };
    }

    public override string ToString()
    {
        string text = IsSuccess ? "Success" : $"Failure {Failure}";

        if (Failure == FailureKind.Business)
        {
            text += $" {ErrorCode}";
        }

        return text;
    }
}
