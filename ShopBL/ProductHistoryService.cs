using System;
using System.Collections.Generic;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopBL;

public class ProductHistoryService : IProductHistoryService
{
    private readonly IProductHistoryRepository _histories;

    public ProductHistoryService(IProductHistoryRepository histories)
    {
        _histories = histories ?? throw new ArgumentNullException(nameof(histories));
    }

    public IEnumerable<ProductHistory> GetAllHistories() => _histories.GetAll();

    public void AddHistory(ProductHistory history)
    {
        if (history == null) throw new ArgumentNullException(nameof(history));

        _histories.Add(history);
    }
}