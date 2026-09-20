using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class HistoryPalletsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.HistoryPallet.Any())
            {
                context.HistoryPallet.AddRange(
                new HistoryPallet
                {
                    Id = 1,
                    PalletNumber = "Q1000",
                    PalletId = SeedIds.Pallet1,
                    DestinationLocationId = 2,
                    Reason = ReasonForPallet.Moved,
                    MovementDate = new DateTime(2025, 2, 2),
                    PerformedBy = "TestUser",
                },
                new HistoryPallet
                {
                    Id = 2,
                    PalletNumber = "Q1001",
                    PalletId = SeedIds.Pallet2,
                    DestinationLocationId = 1,
                    Reason = ReasonForPallet.Moved,
                    MovementDate = new DateTime(2025, 2, 2),
                    PerformedBy = "TestUser",
                },
                new HistoryPallet
                {
                    Id = 3,
                    PalletNumber = "Q1002",
                    PalletId = SeedIds.Pallet3,
                    DestinationLocationId = 3,
                    Reason = ReasonForPallet.Moved,
                    MovementDate = new DateTime(2025, 2, 2),
                    PerformedBy = "TestUser",
                },
                new HistoryPallet
                {
                    Id = 4,
                    PalletNumber = "Q1010",
                    PalletId = SeedIds.Pallet4,
                    DestinationLocationId = 3,
                    Reason = ReasonForPallet.Moved,
                    MovementDate = new DateTime(2025, 2, 2),
                    PerformedBy = "TestUser",
                },
                new HistoryPallet
                {
                    Id = 5,
                    PalletNumber = "Q1000",
                    PalletId = SeedIds.Pallet1,
                    DestinationLocationId = 3,
                    Reason = ReasonForPallet.Moved,
                    MovementDate = new DateTime(2025, 2, 2),
                    PerformedBy = "TestUser",
                }
                );
            }
            context.SaveChanges();
        }
    }
}

