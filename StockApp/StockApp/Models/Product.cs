using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Models;

public class Product
{
    public int Id { get; set; }
    public string Barcode { get; set; }
    public string Name { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
}
