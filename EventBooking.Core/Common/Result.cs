namespace EventBooking.Core.Common;

/// <summary>Outcome kind of a service call. The API layer maps each value to an HTTP status code.</summary>
public enum ResultStatus
{
    Ok = 0,
    NotFound = 1,
    Invalid = 2,       // business/state validation failed  -> 400
    Conflict = 3,      // resource competition lost          -> 409
    Forbidden = 4,     // authenticated but not allowed      -> 403
    Unauthorized = 5   // authentication failed              -> 401
}

/// <summary>
/// A service result without a payload. Services return this instead of throwing for
/// <em>expected</em> failures (not found, slot taken, ...). Real bugs still throw.
/// </summary>
public class Result
{
    public ResultStatus Status { get; }
    public string? Error { get; }
    public bool IsSuccess => Status == ResultStatus.Ok;

    protected Result(ResultStatus status, string? error)
    {
        Status = status;
        Error = error;
    }

    public static Result Ok() => new(ResultStatus.Ok, null);
    public static Result NotFound(string error) => new(ResultStatus.NotFound, error);
    public static Result Invalid(string error) => new(ResultStatus.Invalid, error);
    public static Result Conflict(string error) => new(ResultStatus.Conflict, error);
    public static Result Forbidden(string error) => new(ResultStatus.Forbidden, error);
    public static Result Unauthorized(string error) => new(ResultStatus.Unauthorized, error);
}

/// <summary>A service result that carries a value on success.</summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(ResultStatus.Ok, null) => Value = value;
    private Result(ResultStatus status, string? error) : base(status, error) => Value = default;

    public static Result<T> Ok(T value) => new(value);
    public static new Result<T> NotFound(string error) => new(ResultStatus.NotFound, error);
    public static new Result<T> Invalid(string error) => new(ResultStatus.Invalid, error);
    public static new Result<T> Conflict(string error) => new(ResultStatus.Conflict, error);
    public static new Result<T> Forbidden(string error) => new(ResultStatus.Forbidden, error);
    public static new Result<T> Unauthorized(string error) => new(ResultStatus.Unauthorized, error);
}
