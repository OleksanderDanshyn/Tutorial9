using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;
namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseProductController : ControllerBase
{
    private readonly IWarehouseProductService _warehouseProductService;

    public WarehouseProductController(IWarehouseProductService warehouseProductService)
    {
        _warehouseProductService = warehouseProductService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] AddDataToTable request)
    {
        try
        {
            int newId = await _warehouseProductService.AddProductToWarehouseAsync(request);
            return Ok(new { IdProductWarehouse = newId });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}