using MyWerehouse.Infrastructure.Persistence;
using TestSupport.Seeders.Tables;
using TestSupport.Seeders.Scenarios;

namespace TestSupport
{
    public static class TestDataSeeder
    {
        public static void SeedDatabase(WerehouseDbContext context)
        {
            SeederForIssue.SeedDatabase(context);
            UsersSeeder.SeedDatabase(context);
            NumberCountersSeeder.SeedDatabase(context);
            PickingTasksSeeder.SeedDatabase(context);
            HistoryPalletsSeeder.SeedDatabase(context);
            HistoryPalletDetailsSeeder.SeedDatabase(context);
            ReversePickingsSeeder.SeedDatabase(context);            
        }
    }
}
