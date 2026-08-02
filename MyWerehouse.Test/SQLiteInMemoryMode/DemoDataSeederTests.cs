using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence.Seeding;

namespace MyWerehouse.Test.SQLiteInMemoryMode
{
	public class DemoDataSeederTests : TestBase
	{
		[Fact]
		public async Task SeedAsync_ShouldCreateDemoScenariosOnlyOnce()
		{
			await DemoDataSeeder.SeedAsync(DbContext);

			Assert.Equal(4, await DbContext.Products.CountAsync(p => p.SKU.StartsWith("DEMO-")));
			Assert.Equal(2, await DbContext.Clients.CountAsync(c => c.Email.StartsWith("demo.")));
			Assert.Equal(4, await DbContext.Issues.CountAsync(i => i.IssueNumber >= 900001 && i.IssueNumber <= 900004));
			Assert.Equal(8, await DbContext.Pallets.CountAsync(p => p.PalletNumber.StartsWith("DEMO-")));
			Assert.Equal(3, await DbContext.PickingTasks.CountAsync());
			Assert.Single(await DbContext.ReversePickings.ToListAsync());

			var manualTask = await DbContext.PickingTasks.SingleAsync(t =>
				t.PickingStatus == PickingStatus.Available && t.VirtualPalletId == null);
			Assert.Equal(12, manualTask.RequestedQuantity);

			var loadingIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.SingleAsync(i => i.IssueNumber == 900003);
			Assert.Equal(IssueStatus.ConfirmedToLoad, loadingIssue.IssueStatus);
			Assert.Single(loadingIssue.Pallets);

			var productCount = await DbContext.Products.CountAsync();
			var issueCount = await DbContext.Issues.CountAsync();
			var palletCount = await DbContext.Pallets.CountAsync();

			await DemoDataSeeder.SeedAsync(DbContext);

			Assert.Equal(productCount, await DbContext.Products.CountAsync());
			Assert.Equal(issueCount, await DbContext.Issues.CountAsync());
			Assert.Equal(palletCount, await DbContext.Pallets.CountAsync());
		}
	}
}
