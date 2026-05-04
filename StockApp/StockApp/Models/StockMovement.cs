namespace StockApp.Models;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string MovementType { get; set; } // IN / OUT
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
}