using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class AddressesSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Addresses.Any())
            {
                context.Addresses.AddRange(
                new Address { Id = 10, Country = "ConutryTest", City = "CityTest", Region = "RegionTest", Phone = 123123123, PostalCode = "12ggt", StreetName = "StreetTest", StreetNumber = "12/1", ClientId = 10 },
                new Address { Id = 11, Country = "ConutryTest1", City = "CityTest1", Region = "RegionTest1", Phone = 987987987, PostalCode = "test12ggt", StreetName = "StreetTest1", StreetNumber = "12/11", ClientId = 10 }
                );
            }
            context.SaveChanges();
        }
    }
}

