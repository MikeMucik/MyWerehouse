using MyWerehouse.Domain.Pickings.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class PickingTasksSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.PickingTasks.Any())
            {
                context.PickingTasks.AddRange(
                PickingTask.CreateForSeed(SeedIds.Picking1, SeedIds.VirtualPallet1, SeedIds.Issue2, 20, PickingStatus.Allocated, SeedIds.Product2,
                DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddHours(23).AddDays(-2)), 0),
                PickingTask.CreateForSeed(SeedIds.Picking2, SeedIds.VirtualPallet1, SeedIds.Issue2, 20, PickingStatus.Picked, SeedIds.Product2,
                DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddHours(23).AddDays(-2)), 20),

                PickingTask.CreateForSeed(SeedIds.Picking3, SeedIds.VirtualPallet2, SeedIds.Issue2, 50, PickingStatus.Allocated, SeedIds.Product2,
                DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddHours(23).AddDays(-5)), 0),

                PickingTask.CreateForSeed(SeedIds.Picking4, SeedIds.VirtualPallet3, SeedIds.Issue2, 100, PickingStatus.Allocated, SeedIds.Product1,
                DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddHours(23).AddDays(-2)), 0),
                PickingTask.CreateForSeed(SeedIds.Picking5, SeedIds.VirtualPallet3, SeedIds.Issue2, 10, PickingStatus.Picked, SeedIds.Product1,
                DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddHours(23).AddDays(-2)), 10),
                PickingTask.CreateForSeed(SeedIds.Picking6, SeedIds.VirtualPallet1, SeedIds.Issue2, 10, PickingStatus.Allocated, SeedIds.Product2,
                DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddHours(23).AddDays(-2)), 0)
                );
            }
            context.SaveChanges();
        }
    }
}

