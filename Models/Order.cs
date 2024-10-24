namespace GrpcCrudExample.Models;

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public required string CustomerName { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
