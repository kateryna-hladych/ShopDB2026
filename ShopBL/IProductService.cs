using System.Collections.Generic;
using ShopDB.DAL.EF.Entities;

namespace ShopBL;

public interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    void AddProduct(Product product);
    void DeleteProduct(int productId); 
    IEnumerable<Category> GetAllCategories();
}