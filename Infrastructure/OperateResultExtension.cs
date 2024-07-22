namespace Infrastructure;

public static class OperateResultExtension
{
    public static OperateResult Success(string errorMessage = "")
    {
        return new OperateResult(true, errorMessage);
    }

    public static OperateResult Fail(string errorMessage = "")
    {
        return new OperateResult(false, errorMessage);
    }

    public static OperateResult<T> Success<T>(string errorMessage = "", T? data = default)
    {
        return new OperateResult<T>(true, errorMessage, data);
    }

    public static OperateResult<T> Fail<T>(string errorMessage = "", T? data = default)
    {
        return new OperateResult<T>(false, errorMessage, data);
    }
}