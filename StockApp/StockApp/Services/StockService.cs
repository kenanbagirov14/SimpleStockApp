using Microsoft.Data.Sqlite;
using StockApp.Data;

namespace StockApp.Services;

public class StockService
{
    public void StockIn(int productId, decimal quantity)
    {
        ChangeStock(productId, quantity, "IN");
    }

    public void StockOut(int productId, decimal quantity)
    {
        ChangeStock(productId, quantity, "OUT");
    }

    private void ChangeStock(int productId, decimal quantity, string type)
    {
        using var connection = new SqliteConnection(Database.ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            var updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;

            if (type == "IN")
            {
                updateCommand.CommandText =
                @"
                UPDATE Products
                SET Quantity = Quantity + $quantity
                WHERE Id = $productId;
                ";
            }
            else
            {
                updateCommand.CommandText =
                @"
                UPDATE Products
                SET Quantity = Quantity - $quantity
                WHERE Id = $productId
                  AND Quantity >= $quantity;
                ";
            }

            updateCommand.Parameters.AddWithValue("$quantity", quantity);
            updateCommand.Parameters.AddWithValue("$productId", productId);

            var affectedRows = updateCommand.ExecuteNonQuery();

            if (affectedRows == 0)
                throw new Exception("Stok kifayət etmir.");

            var movementCommand = connection.CreateCommand();
            movementCommand.Transaction = transaction;
            movementCommand.CommandText =
            @"
            INSERT INTO StockMovements (ProductId, MovementType, Quantity, CreatedDate)
            VALUES ($productId, $type, $quantity, $createdDate);
            ";

            movementCommand.Parameters.AddWithValue("$productId", productId);
            movementCommand.Parameters.AddWithValue("$type", type);
            movementCommand.Parameters.AddWithValue("$quantity", quantity);
            movementCommand.Parameters.AddWithValue("$createdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            movementCommand.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}