using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class ClientsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(
                new Client { Id = 10, Name = "ClientTest", Email = "client@op.pl", Description = "ClientDescription", FullName = "FullNameTestAddress" },
                new Client { Id = 11, Name = "ClientTest1", Email = "client1@op.pl", Description = "ClientDescription1", FullName = "FullNameTestAddress1" },
                new Client { Id = 989, Name = "ClientTest2", Email = "client2@op.pl", Description = "ClientDescription2", FullName = "FullNameTestAddress2" }
                );
            }
            context.SaveChanges();
        }
    }
}

