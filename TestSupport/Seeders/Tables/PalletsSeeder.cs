using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pickings.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class PalletsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Pallets.Any())
            {
                context.Pallets.AddRange(
                Pallet.CreateForSeed(SeedIds.Pallet1, "Q1000", new DateTime(2020, 1, 1), 1, PalletStatus.Available, SeedIds.Receipt1, SeedIds.Issue2),
                Pallet.CreateForSeed(SeedIds.Pallet2, "Q1001", new DateTime(2020, 1, 1), 1, PalletStatus.OnHold, SeedIds.Receipt1, SeedIds.Issue2),
                Pallet.CreateForSeed(SeedIds.Pallet3, "Q1002", new DateTime(2020, 1, 1), 3, PalletStatus.Available, SeedIds.Receipt2, null),
                Pallet.CreateForSeed(SeedIds.Pallet4, "Q1010", new DateTime(2025, 1, 1), 3, PalletStatus.Damaged, SeedIds.Receipt2, null),
                Pallet.CreateForSeed(SeedIds.Pallet5, "Q1100", new DateTime(2025, 1, 1), 3, PalletStatus.ToPicking, SeedIds.Receipt2, null),
                Pallet.CreateForSeed(SeedIds.Pallet6, "Q1101", new DateTime(2025, 1, 5), 3, PalletStatus.ToPicking, SeedIds.Receipt2, null),
                Pallet.CreateForSeed(SeedIds.Pallet7, "Q2000", new DateTime(2025, 1, 1), 3, PalletStatus.ToIssue, SeedIds.Receipt2, SeedIds.Issue2),
                //PalletSource to picking
                Pallet.CreateForSeed(SeedIds.Pallet8, "Q1200", new DateTime(2025, 2, 1), 3, PalletStatus.ToPicking, SeedIds.Receipt2, null),
                //PickingPallet
                Pallet.CreateForSeed(SeedIds.Pallet9, "Q5000", new DateTime(2025, 2, 1), 3, PalletStatus.Picking, null, null)
                );
            }
            context.SaveChanges();
            if (!context.VirtualPallets.Any())
            {
                context.VirtualPallets.AddRange(
                VirtualPallet.CreateForSeed(SeedIds.VirtualPallet1, SeedIds.Pallet5, 200, 3, TestDates.UtcNow.AddDays(-1)),
                VirtualPallet.CreateForSeed(SeedIds.VirtualPallet2, SeedIds.Pallet6, 150, 3, new DateTime(2024, 6, 6)),
                VirtualPallet.CreateForSeed(SeedIds.VirtualPallet3, SeedIds.Pallet8, 300, 3, TestDates.UtcNow.AddDays(-1)));
            }
            context.SaveChanges();
        }
    }
}

