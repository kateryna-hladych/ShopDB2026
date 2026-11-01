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

        ValidateFields(product.Name, product.PurchasePrice, product.SellingPrice, product.StockQuantity);

        if (_categories.GetById(product.CategoryId) == null)
            throw new BusinessRuleException("Категорію не знайдено!");

        _products.Add(product);
    }

    public void UpdateProduct(int productId, string name, decimal purchasePrice, decimal sellingPrice)
    {
        var product = _products.GetById(productId);
        if (product == null)
            throw new BusinessRuleException("Товар не знайдено!");

        name = (name ?? string.Empty).Trim();

        
        ValidateFields(name, purchasePrice, sellingPrice, product.StockQuantity);

        product.Name = name;
        product.PurchasePrice = purchasePrice;
        product.SellingPrice = sellingPrice;

        _products.Update(product);
    }

    public void SetProductBlocked(int productId, bool isBlocked)
    {
        var product = _products.GetById(productId);
        if (product == null)
            throw new BusinessRuleException("Товар не знайдено!");

        product.IsBlocked = isBlocked;
        _products.Update(product);
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

    private static void ValidateFields(string name, decimal purchasePrice, decimal sellingPrice, int stock)
    {
        if (name.Length == 0)
            throw new BusinessRuleException("Назва товару не може бути порожньою!");
        if (purchasePrice <= 0)
            throw new BusinessRuleException("Закупівельна ціна має бути більшою за 0!");
        if (sellingPrice <= 0)
            throw new BusinessRuleException("Ціна продажу має бути більшою за 0!");
        if (stock < 0)
            throw new BusinessRuleException("Кількість не може бути від'ємною!");
    }
}