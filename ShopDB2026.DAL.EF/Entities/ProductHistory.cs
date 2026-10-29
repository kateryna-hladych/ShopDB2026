namespace ShopDB.DAL.EF.Entities;

public partial class ProductHistory
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int AddedQuantity { get; set; }

    public string Comment { get; set; } = string.Empty;

    public virtual Product Product { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}