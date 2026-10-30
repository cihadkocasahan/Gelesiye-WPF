namespace GelesiyeWPF.Common;

/// <summary>
/// Servis katmanından UI katmanına dönen standart işlem sonucu.
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<string> Errors { get; }

    protected ServiceResult(bool isSuccess, string? errorMessage, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors?.ToList() ?? (errorMessage != null ? new List<string> { errorMessage } : new List<string>());
    }

    public static ServiceResult Success() => new(true, null);
    public static ServiceResult Fail(string message) => new(false, message);
    public static ServiceResult Fail(IEnumerable<string> errors)
    {
        var errList = errors.ToList();
        return new(false, string.Join("\n", errList), errList);
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    protected ServiceResult(bool isSuccess, T? data, string? errorMessage, IEnumerable<string>? errors = null)
        : base(isSuccess, errorMessage, errors)
    {
        Data = data;
    }

    public static ServiceResult<T> Success(T data) => new(true, data, null);
    public new static ServiceResult<T> Fail(string message) => new(false, default, message);
    public new static ServiceResult<T> Fail(IEnumerable<string> errors)
    {
        var errList = errors.ToList();
        return new(false, default, string.Join("\n", errList), errList);
    }
}
