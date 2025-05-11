namespace Tutorial9.Model;

public class WarehouseProduct
{
    public int productWarehouseId { get; set; }
    public int idWarehouse { get; set; }
    public int idProduct { get; set; }
    public int idOrder { get; set; }
    public int amount { get; set; }
    public double price { get; set; }
    public DateTime createdAt { get; set; }
}