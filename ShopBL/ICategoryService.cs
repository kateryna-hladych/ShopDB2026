using System.Collections.Generic;
using ShopDB.DAL.EF.Entities;

namespace ShopBL;

public interface ICategoryService
{
    IEnumerable<Category> GetAllCategories();
    void AddCategory(Category category);
    void DeleteCategory(int categoryId);
    void UpdateCategory(Category category);
}