namespace ShopDB.DTO;

public class ProductHistoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AddedQuantity { get; set; }  
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}