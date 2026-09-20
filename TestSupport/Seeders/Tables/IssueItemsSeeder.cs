using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class IssueItemsSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.IssueItems.Any())
            {
                var issue = context.Issues.Single(value => value.Id == SeedIds.Issue2);
                var items = new[]
                {
                    IssueItem.CreateForSeed(1, SeedIds.Issue2, SeedIds.Product1, 150, DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), TestDates.TodayDateTime),
                    IssueItem.CreateForSeed(2, SeedIds.Issue2, SeedIds.Product2, 400, DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(3)), TestDates.TodayDateTime)
                };
                context.IssueItems.AddRange(items);
                // CreateForSeed does not set IssueId; EF sets the foreign key through the relationship.
                foreach (var item in items)
                    context.Entry(item).Reference(value => value.Issue).CurrentValue = issue;
            }
            context.SaveChanges();
        }
    }
}

