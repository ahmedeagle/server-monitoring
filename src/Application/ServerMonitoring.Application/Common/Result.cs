namespace ServerMonitoring.Application.Common;

/// <summary>
/// Generic result wrapper for query responses
/// </summary>
public class Result<T>
{
    public T Data { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            Data = data,
            IsSuccess = true
        };
    }

    public static Result<T> Failure(string message, List<string>? errors = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }
}
