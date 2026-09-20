using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class CategoriesSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                new Category { Id = 1, Name = "TestCategory", IsDeleted = false },
                new Category { Id = 2, Name = "TestCategory1", IsDeleted = false },
                new Category { Id = 3, Name = "ToDeleted", IsDeleted = false },
                new Category { Id = 4, Name = "SwitchOff", IsDeleted = true }
                );
            }
            context.SaveChanges();
        }
    }
}

