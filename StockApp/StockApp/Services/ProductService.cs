using Microsoft.Data.Sqlite;
using StockApp.Data;
using StockApp.Models;

namespace StockApp.Services;

public class ProductService
{
    public void AddProduct(string barcode, string name,string quayntity)
    {
        using var connection = new SqliteConnection(Database.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
        INSERT INTO Products (Barcode, Name, Quantity, CreatedDate)
        VALUES ($barcode, $name, $quayntity, $createdDate);
        ";

        command.Parameters.AddWithValue("$barcode", barcode);
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$quayntity", quayntity);
        command.Parameters.AddWithValue("$createdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        command.ExecuteNonQuery();
    }

    public Product GetByBarcode(string barcode)
    {
        using var connection = new SqliteConnection(Database.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
        SELECT Id, Barcode, Name, Quantity, CreatedDate
        FROM Products
        WHERE Barcode = $barcode;
        ";

        command.Parameters.AddWithValue("$barcode", barcode);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new Product
        {
            Id = reader.GetInt32(0),
            Barcode = reader.GetString(1),
            Name = reader.GetString(2),
            Quantity = reader.GetDecimal(3),
            CreatedDate = DateTime.Parse(reader.GetString(4))
        };
    }

    public List<Product> GetAll()
    {
        var list = new List<Product>();

        using var connection = new SqliteConnection(Database.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
        SELECT Id, Barcode, Name, Quantity, CreatedDate
        FROM Products
        ORDER BY Id DESC;
        ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new Product
            {
                Id = reader.GetInt32(0),
                Barcode = reader.GetString(1),
                Name = reader.GetString(2),
                Quantity = reader.GetDecimal(3),
                CreatedDate = DateTime.Parse(reader.GetString(4))
            });
        }

        return list;
    }

    public void UpdateProduct(int id, string barcode, string name)
    {
        using var connection = new SqliteConnection(Database.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
    UPDATE Products
    SET Barcode = $barcode,
        Name = $name
    WHERE Id = $id;
    ";

        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$barcode", barcode);
        command.Parameters.AddWithValue("$name", name);

        command.ExecuteNonQuery();
    }

    public void DeleteProduct(int id)
    {
        using var connection = new SqliteConnection(Database.ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            var deleteMovements = connection.CreateCommand();
            deleteMovements.Transaction = transaction;
            deleteMovements.CommandText =
            @"
        DELETE FROM StockMovements
        WHERE ProductId = $id;
        ";
            deleteMovements.Parameters.AddWithValue("$id", id);
            deleteMovements.ExecuteNonQuery();

            var deleteProduct = connection.CreateCommand();
            deleteProduct.Transaction = transaction;
            deleteProduct.CommandText =
            @"
        DELETE FROM Products
        WHERE Id = $id;
        ";
            deleteProduct.Parameters.AddWithValue("$id", id);
            deleteProduct.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}