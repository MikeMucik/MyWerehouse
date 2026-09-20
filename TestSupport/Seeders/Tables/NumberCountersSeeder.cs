using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class NumberCountersSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.NumberCounters.Any())
            {
                context.NumberCounters.Add(
                new NumberCounter { Name = "Pallet", NextNumber = 5001 });
            }
            context.SaveChanges();
        }
    }
}

