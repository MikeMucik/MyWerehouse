using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Inventories.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class ProductsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                Product.CreateForSeed(SeedIds.Product1, "Test", "0987654321", new DateTime(2025, 05, 01), 1, false, 56, 10, 20, 30, 2, "TestDetails"),
                Product.CreateForSeed(SeedIds.Product2, "TestD", "fghtredfg", new DateTime(2025, 05, 01), 1, false, 50, 20, 40, 60, 3, "TestDetails 11"),
                Product.CreateForTests(SeedIds.Product989, "NotAdded", "fghtredfg1", new DateTime(2025, 05, 01), 1, false, 112)

                );
            }
            context.SaveChanges();
            if (!context.Inventories.Any())
            {
                context.Inventories.AddRange(
                Inventory.CreateStockItem(SeedIds.Product1, 10, new DateTime(2025, 5, 6)),
                Inventory.CreateStockItem(SeedIds.Product2, 0, new DateTime(2025, 5, 6)));
            }
            context.SaveChanges();
        }
    }
}

