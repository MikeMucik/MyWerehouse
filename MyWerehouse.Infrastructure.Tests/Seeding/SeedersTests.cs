using Microsoft.EntityFrameworkCore;
using TestSupport;
using TestSupport.Seeders;
using TestSupport.Seeders.Scenarios;

namespace MyWerehouse.Infrastructure.Tests.Seeding;

public class SeedersTests
{
    [Fact]
    public void CategoryScenario_ShouldSeedOnlyCategories_AndAllowRepeatedExecution()
    {
        using var database = new SqliteTestDatabase();
        SeederForCategory.SeedDatabase(database.DbContext);
        SeederForCategory.SeedDatabase(database.DbContext);

        using var reader = database.CreateNewContext();
        Assert.Equal(4, reader.Categories.Count());
        Assert.Single(reader.Categories.Where(category => category.IsDeleted));
        Assert.Empty(reader.Products);
        Assert.Empty(reader.Clients);
        Assert.Empty(reader.Pallets);
        Assert.Empty(reader.Issues);
    }

	//Client
	[Fact]
	public void ClientScenario_ShouldSeedRelatedData_AndAllowRepeatedExecution()
	{
		using var database = new SqliteTestDatabase();
		SeederForClient.SeedDatabase(database.DbContext);
		SeederForClient.SeedDatabase(database.DbContext);

		using var reader = database.CreateNewContext();
		Assert.Equal(3, reader.Clients
            .Include(a=>a.Addresses)
            .Count());
		Assert.Empty(reader.Products);
		Assert.Empty(reader.Pallets);
		Assert.Empty(reader.Issues);
	}
	[Fact]
    public void IssueScenario_ShouldSeedRelatedData_WithoutPickingTasksOrHistory()
    {
        using var database = new SqliteTestDatabase();
        SeederForIssue.SeedDatabase(database.DbContext);
        SeederForIssue.SeedDatabase(database.DbContext);

        using var reader = database.CreateNewContext();
        var issue = reader.Issues
            .Include(value => value.Client)
            .Include(value => value.IssueItems).ThenInclude(item => item.Product)
            .Include(value => value.Pallets).ThenInclude(pallet => pallet.ProductsOnPallet)
            .Single();

        Assert.Equal(SeedIds.Issue2, issue.Id);
        Assert.Equal("ClientTest1", issue.Client.Name);
        Assert.Equal(2, issue.IssueItems.Count);
        Assert.All(issue.IssueItems, item => Assert.NotNull(item.Product));
        Assert.Equal(3, issue.Pallets.Count);
        Assert.All(issue.Pallets, pallet => Assert.NotEmpty(pallet.ProductsOnPallet));
        Assert.Equal(3, reader.ProductDetails.Count());
        Assert.Equal(2, reader.Inventories.Count());
        Assert.Equal(3, reader.VirtualPallets.Count());
        Assert.Empty(reader.PickingTasks);
        Assert.Empty(reader.ReversePickings);
        Assert.Empty(reader.HistoryPallet);
    }

    [Fact]
    public void FullScenario_ShouldCompletePartialSeed_AndAllowRepeatedExecution()
    {
        using var database = new SqliteTestDatabase();
        SeederForCategory.SeedDatabase(database.DbContext);
        TestDataSeeder.SeedDatabase(database.DbContext);
        TestDataSeeder.SeedDatabase(database.DbContext);

        using var reader = database.CreateNewContext();
        Assert.Equal(4, reader.Categories.Count());
        Assert.Equal(3, reader.Products.Count());
        Assert.Equal(2, reader.IssueItems.Count());
        Assert.Equal(9, reader.Pallets.Count());
        Assert.Equal(11, reader.ProductOnPallet.Count());
        Assert.Equal(6, reader.PickingTasks.Count());
        Assert.Equal(5, reader.HistoryPallet.Count());
        Assert.Equal(6, reader.HistoryPalletDetails.Count());
        Assert.Equal(2, reader.ReversePickings.Count());
        Assert.Equal(3, reader.Users.Count());
        Assert.Equal("Pallet", reader.NumberCounters.Single().Name);
    }
}
