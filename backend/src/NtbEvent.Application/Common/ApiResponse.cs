namespace NtbEvent.Application.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public List<string>? Errors { get; init; }
    public bool? RequiresApproval { get; init; }

    public static ApiResponse<T> Success(T data, string? message = null, bool? requiresApproval = null) => new()
    {
        IsSuccess = true,
        Message = message,
        Data = data,
        RequiresApproval = requiresApproval
    };

    public static ApiResponse<T> Failure(string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string? message = null) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public new static ApiResponse Failure(string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}
