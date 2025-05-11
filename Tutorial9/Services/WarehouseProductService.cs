using System.Data;
using System.Data.Common;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class WarehouseProductService : IWarehouseProductService
{
    private readonly IConfiguration _configuration;

    public WarehouseProductService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddProductToWarehouseAsync(AddDataToTable request)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        try
        {
            //---------------------------1
            command.CommandText = "SELECT count(*) FROM Product WHERE IdProduct = @idProduct";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@idProduct", request.IdProduct);
            int productExist = (int) await command.ExecuteScalarAsync();
            if (productExist == 0)
            {
                throw new InvalidOperationException("Product not found");
            }
            
            command.CommandText = "SELECT count(*) FROM Warehouse WHERE IdWarehouse = @idWarehouse";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@idWarehouse", request.IdWarehouse);
            int warehouseExist = (int) await command.ExecuteScalarAsync();
            if (warehouseExist == 0)
            {
                throw new InvalidOperationException("Warehouse not found");
            }

            if (request.Amount < 1)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }
            //---------------------------
            
            //---------------------------2
            command.CommandText = @"
                SELECT TOP 1 IdOrder
                FROM Order
                WHERE IdProduct = @idProduct
                AND Amount >= @amount
                AND CreatedAt <= @createdAt;";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@idProduct", request.IdProduct);
            command.Parameters.AddWithValue("@amount", request.Amount);
            command.Parameters.AddWithValue("@createdAt", request.CreatedAt);
            
            object idOrderObj = await command.ExecuteScalarAsync();
            if (idOrderObj == null)
            {
                throw new InvalidOperationException("No order found");
            }
            int idOrder = (int)idOrderObj;
            //---------------------------
            
            //---------------------------3
            command.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @idOrder;";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@idOrder", idOrder);
            
            int alreadyDone = (int) await command.ExecuteScalarAsync();
            if (alreadyDone > 0)
            {
                throw new InvalidOperationException("Order already done");
            }
            //---------------------------
            
            //---------------------------4
            command.CommandText = "UPDATE [Order] SET FulfilledAt = @now WHERE IdOrder = @idOrder;";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@now", DateTime.Now);
            command.Parameters.AddWithValue("@idOrder", idOrder);
            await command.ExecuteNonQueryAsync();
            //---------------------------
            
            //---------------------------5
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @idProduct;";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@idProduct", request.IdProduct);
            decimal price = (decimal)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("No price found"));
            decimal totalPrice = price * request.Amount;
            
            command.CommandText =
                @"INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    OUTPUT INSERTED.IdProductWarehouse
                    VALUES (@idWarehouse, @idProduct, @idOrder, @amount, @price, @createdAt);
                    ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@idWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@idProduct", request.IdProduct);
            command.Parameters.AddWithValue("@idOrder", idOrder);
            command.Parameters.AddWithValue("@amount", request.Amount);
            command.Parameters.AddWithValue("@price", totalPrice);
            command.Parameters.AddWithValue("@createdAt", DateTime.Now);
            
            int newId = (int)(await command.ExecuteScalarAsync() ?? throw new Exception("Insert failed"));

            await transaction.CommitAsync();
            return newId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }

    public async Task procedureAsync()
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WarehouseDb"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();

        command.CommandText = "asdasds";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@idWarehouse", 1);
        command.Parameters.AddWithValue("@idProduct", 1);
        command.Parameters.AddWithValue("@amount", 1);
        command.Parameters.AddWithValue("@createdAt", DateTime.Now);
        
        await command.ExecuteNonQueryAsync();
    }
}