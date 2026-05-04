using Microsoft.Data.Sqlite;

namespace StockApp.Data;

public static class Database
{
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stock.db");

    public static string ConnectionString => $"Data Source={DbPath}";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        @"
        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Barcode TEXT NOT NULL UNIQUE,
            Name TEXT NOT NULL,
            Quantity REAL NOT NULL DEFAULT 0,
            CreatedDate TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS StockMovements (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProductId INTEGER NOT NULL,
            MovementType TEXT NOT NULL,
            Quantity REAL NOT NULL,
            CreatedDate TEXT NOT NULL,
            FOREIGN KEY(ProductId) REFERENCES Products(Id)
        );
        ";

        command.ExecuteNonQuery();
    }
}