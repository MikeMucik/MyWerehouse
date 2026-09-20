using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class ProductsOnPalletSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.ProductOnPallet.Any())
            {
                context.ProductOnPallet.AddRange(
                ProductOnPallet.CreateForSeed(1, SeedIds.Product1, SeedIds.Pallet1, 50, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),

                ProductOnPallet.CreateForSeed(2, SeedIds.Product1, SeedIds.Pallet2, 100, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(3, SeedIds.Product2, SeedIds.Pallet1, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(4, SeedIds.Product2, SeedIds.Pallet4, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(5, SeedIds.Product2, SeedIds.Pallet3, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(6, SeedIds.Product2, SeedIds.Pallet5, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(7, SeedIds.Product2, SeedIds.Pallet7, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(8, SeedIds.Product2, SeedIds.Pallet6, 150, new DateTime(2024, 3, 3), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(9, SeedIds.Product1, SeedIds.Pallet8, 300, new DateTime(2024, 4, 4), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),

                ProductOnPallet.CreateForSeed(10, SeedIds.Product1, SeedIds.Pallet9, 10, new DateTime(2024, 4, 4), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366))),
                ProductOnPallet.CreateForSeed(11, SeedIds.Product2, SeedIds.Pallet9, 20, new DateTime(2024, 4, 4), DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366)))
                );
            }
            context.SaveChanges();
        }
    }
}

