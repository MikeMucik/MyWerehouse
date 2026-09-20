using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class ReversePickingsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.ReversePickings.Any())
            {
                context.ReversePickings.AddRange(
                ReversePickingTask.CreateForSeed(SeedIds.ReversePicking1, SeedIds.Pallet9, null, SeedIds.Product1, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)), 10, SeedIds.Picking2, "UserR", TestDates.Today),
                ReversePickingTask.CreateForSeed(SeedIds.ReversePicking2, SeedIds.Pallet9, null, SeedIds.Product1, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)), 10, SeedIds.Picking5, "UserR", TestDates.Today)
                );
            }
            context.SaveChanges();
        }
    }
}

