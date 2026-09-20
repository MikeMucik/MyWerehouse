using Microsoft.AspNetCore.Identity;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class UsersSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                new IdentityUser { Id = "TestUser", UserName = "TestUser" },
                new IdentityUser { Id = "U002", UserName = "TestUser2" },
                new IdentityUser { Id = "UserR", UserName = "TestUserRR" }

                );
            }
            context.SaveChanges();
        }
    }
}

