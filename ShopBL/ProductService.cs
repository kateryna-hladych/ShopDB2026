using System;
using System.Collections.Generic;
using System.Linq;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopBL;

public class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly IProductHistoryRepository _histories;
    private readonly ICategoryRepository _categories;

    public ProductService(
        IProductRepository products,
        IProductHistoryRepository histories,
        ICategoryRepository categories)
    {
        _products = products ?? throw new ArgumentNullException(nameof(products));
        _histories = histories ?? throw new ArgumentNullException(nameof(histories));
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
    }

    public IEnumerable<Product> GetAllProducts() => _products.GetAll();

    public IEnumerable<Category> GetAllCategories() => _categories.GetAll();

    public void AddProduct(Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));

        
        product.Name = (product.Name ?? string.Empty).Trim();

        if (product.Name.Length == 0)
            throw new BusinessRuleException("Назва товару не може бути порожньою!");
        if (product.PurchasePrice < 0)
            throw new BusinessRuleException("Закупівельна ціна не може бути від'ємною!");
        if (product.SellingPrice <= 0)
            throw new BusinessRuleException("Ціна продажу має бути більшою за 0!");
        if (product.StockQuantity < 0)
            throw new BusinessRuleException("Кількість не може бути від'ємною!");
        if (_categories.GetById(product.CategoryId) == null)
            throw new BusinessRuleException("Категорію не знайдено!");

        _products.Add(product);
    }

    public void DeleteProduct(int productId)
    {
        var product = _products.GetById(productId);
        if (product == null)
        {
            return;
        }

        
        foreach (var history in _histories.GetByProductId(productId).ToList())
        {
            _histories.Delete(history.Id);
        }

        _products.Delete(productId);
    }
}