using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class ReceiptsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Receipts.Any())
            {
                context.Receipts.AddRange(
                Receipt.CreateForSeed(SeedIds.Receipt1, 1, 10, "U001", new DateTime(2023, 3, 3), ReceiptStatus.Verified, 1),
                Receipt.CreateForSeed(SeedIds.Receipt2, 2, 11, "U002", new DateTime(2023, 4, 4), ReceiptStatus.Verified, 1)
                );
            }
            context.SaveChanges();
        }
    }
}

