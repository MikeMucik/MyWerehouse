using MyWerehouse.Infrastructure.Persistence;
using TestSupport.Seeders.Tables;

namespace TestSupport.Seeders.Scenarios
{
    public static class SeederForIssue
    {
        public static void SeedDatabase(WerehouseDbContext context)
        {
            ClientsSeeder.SeedDatabase(context);
            AddressesSeeder.SeedDatabase(context);
            CategoriesSeeder.SeedDatabase(context);
            ProductsSeeder.SeedDatabase(context);
            LocationsSeeder.SeedDatabase(context);
            ReceiptsSeeder.SeedDatabase(context);
            IssuesSeeder.SeedDatabase(context);
            IssueItemsSeeder.SeedDatabase(context);
            PalletsSeeder.SeedDatabase(context);
            ProductsOnPalletSeeder.SeedDatabase(context);
        }
    }
}
