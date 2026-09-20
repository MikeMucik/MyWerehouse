using MyWerehouse.Infrastructure.Persistence;
using TestSupport.Seeders.Tables;

namespace TestSupport.Seeders.Scenarios
{
    public static class SeederForCategory
    {
        public static void SeedDatabase(WerehouseDbContext context)
        {
            CategoriesSeeder.SeedDatabase(context);
        }
    }
}
