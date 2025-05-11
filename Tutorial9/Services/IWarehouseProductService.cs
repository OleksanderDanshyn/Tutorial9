using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IWarehouseProductService
{
    Task<int> AddProductToWarehouseAsync(AddDataToTable request);
    Task procedureAsync();
}