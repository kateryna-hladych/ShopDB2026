namespace ShopDB.DAL.EF.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int StockQuantity { get; set; }

    public bool IsBlocked { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductHistory> ProductHistories { get; set; } = new List<ProductHistory>();
}