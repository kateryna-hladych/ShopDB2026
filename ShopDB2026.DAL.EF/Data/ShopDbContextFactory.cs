using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopDB.DAL.EF.Data
{
    public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
    {
        public ShopDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-O20AE25;Database=ShopDB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            return new ShopDbContext(optionsBuilder.Options);
        }
    }
}