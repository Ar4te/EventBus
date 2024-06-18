namespace WebApplication1.Application.Queries;

public interface ITestQueries
{
    Task<TestDto> GetOrderAsync(int id);
}
