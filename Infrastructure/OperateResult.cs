namespace Infrastructure;

public class OperateResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public OperateResult()
    {
        IsSuccess = false;
    }

    public OperateResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}

public class OperateResult<T> : OperateResult
{
    public T? Data { get; set; }

    public OperateResult() : base()
    {
    }

    public OperateResult(bool isSuccess, string errorMessage, T? data = default) : base(isSuccess, errorMessage)
    {
        Data = data;
    }
}