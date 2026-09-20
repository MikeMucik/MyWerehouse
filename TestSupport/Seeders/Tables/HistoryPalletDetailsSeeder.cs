using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class HistoryPalletDetailsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.HistoryPalletDetails.Any())
            {
                context.HistoryPalletDetails.AddRange(
                new HistoryPalletDetail
                {
                    Id = 1,
                    HistoryPalletId = 1,
                    ProductId = SeedIds.Product1,
                    QuantityChange = 100,
                },
                new HistoryPalletDetail
                {
                    Id = 6,
                    HistoryPalletId = 1,
                    ProductId = SeedIds.Product2,
                    QuantityChange = 1,
                },
                new HistoryPalletDetail
                {
                    Id = 2,
                    HistoryPalletId = 2,
                    ProductId = SeedIds.Product2,
                    QuantityChange = 1,
                },
                new HistoryPalletDetail
                {
                    Id = 3,
                    HistoryPalletId = 3,
                    ProductId = SeedIds.Product2,
                    QuantityChange = 1,
                },
                new HistoryPalletDetail
                {
                    Id = 4,
                    HistoryPalletId = 4,
                    ProductId = SeedIds.Product1,
                    QuantityChange = 1,
                },
                new HistoryPalletDetail
                {
                    Id = 5,
                    HistoryPalletId = 5,
                    ProductId = SeedIds.Product1,
                    QuantityChange = 1,
                });
            }
            context.SaveChanges();
        }
    }
}

