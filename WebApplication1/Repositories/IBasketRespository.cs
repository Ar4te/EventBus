using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IBasketRespository
{
    Task<CustomerBasket?> GetBusketAsync(string customerId);
}
