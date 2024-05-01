namespace Stock.API.Models
{
  public class StockReservation
  {
    public int Quantity { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public bool Applied { get; set; } = false;


  }
}
