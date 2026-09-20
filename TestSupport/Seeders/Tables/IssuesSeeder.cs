using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport.Seeders.Tables
{
    public static class IssuesSeeder
    {
        // The scenario seeder prepares the required related records.
        public static void SeedDatabase(WerehouseDbContext context)
        {
            if (!context.Issues.Any())
            {
                context.Issues.Add(
                Issue.CreateForSeed(SeedIds.Issue2, 2, 11, TestDates.UtcNow.AddDays(-5), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(1)), "U002", IssueStatus.New, null));
            }
            context.SaveChanges();
        }
    }
}

