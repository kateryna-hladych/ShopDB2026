using System;
using System.Collections.Generic;
using System.Linq;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopBL;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IProductRepository _products;

    public CategoryService(ICategoryRepository categories, IProductRepository products)
    {
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _products = products ?? throw new ArgumentNullException(nameof(products));
    }

    public IEnumerable<Category> GetAllCategories() => _categories.GetAll();

    public void AddCategory(Category category)
    {
        NormalizeAndValidate(category);
        _categories.Add(category);
    }

    public void UpdateCategory(Category category)
    {
        NormalizeAndValidate(category);
        _categories.Update(category);
    }

    public void DeleteCategory(int categoryId)
    {
        var category = _categories.GetById(categoryId);
        if (category == null)
        {
            return;
        }

        if (_products.GetAll().Any(p => p.CategoryId == categoryId))
        {
            throw new BusinessRuleException("Неможливо видалити категорію, до якої прив'язані товари!");
        }

        _categories.Delete(categoryId);
    }

    
    private static void NormalizeAndValidate(Category category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category));

        category.Name = (category.Name ?? string.Empty).Trim();

        if (category.Name.Length == 0)
            throw new BusinessRuleException("Назва категорії не може бути порожньою!");
    }
}