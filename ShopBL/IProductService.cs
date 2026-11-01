using System.Collections.Generic;
using ShopDB.DAL.EF.Entities;

namespace ShopBL;

public interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    void AddProduct(Product product);
    void UpdateProduct(int productId, string name, decimal purchasePrice, decimal sellingPrice);
    void SetProductBlocked(int productId, bool isBlocked);
    void DeleteProduct(int productId);
    IEnumerable<Category> GetAllCategories();
}