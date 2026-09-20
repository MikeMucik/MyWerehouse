using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class LocationsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            var location1 = new Location { Id = 1, Aisle = 1, Bay = 2, Position = 3, Height = 4 };
            var location2 = new Location { Id = 2, Aisle = 2, Bay = 3, Position = 4, Height = 5 };
            var location3 = new Location { Id = 3, Aisle = 24, Bay = 3, Position = 4, Height = 5 };
            var location4 = new Location { Id = 20, Aisle = 3, Bay = 3, Position = 4, Height = 5 };
            if (!context.Locations.Any())
            {
                context.Locations.AddRange(location1, location2, location3, location4);
            }
            context.SaveChanges();
        }
    }
}

