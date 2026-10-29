using System.Collections.Generic;
using ShopDB.DAL.EF.Entities;

namespace ShopBL;

public interface IProductHistoryService
{
    IEnumerable<ProductHistory> GetAllHistories();
    void AddHistory(ProductHistory history);
}