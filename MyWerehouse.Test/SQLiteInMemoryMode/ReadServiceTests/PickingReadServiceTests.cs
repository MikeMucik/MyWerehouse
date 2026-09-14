using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Server.ServicesToInfrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class PickingReadServiceTests : TestBase
	{
		private static readonly Guid IssueId =
			Guid.Parse("11111111-2111-1111-1111-111111111111");
		private static readonly Guid ProductId1 =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ProductId2 =
			Guid.Parse("00000000-0000-0000-0002-000000000000");
		private static readonly Guid ProductWithoutPickingTasksId =
			Guid.Parse("00000000-0000-0000-0989-000000000000");
		private static readonly Guid PalletIdWithoutVirtualPallet =
			Guid.Parse("00000000-0001-1111-0000-000000000000");
		private static readonly Guid PalletIdWithPickingTasks =
			Guid.Parse("00000000-0005-1111-0000-000000000000");
		private static readonly Guid SecondPalletIdWithPickingTasks =
			Guid.Parse("00000000-0008-1111-0000-000000000000");

		private readonly PickingReadService _pickingReadService;

		public PickingReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_pickingReadService = new PickingReadService(DbContext);
		}

		[Fact]
		public async Task GetPickingTaskFlat_ShouldReturnGroupedPickingTree_WhenTasksExist()
		{
			// Act
			var result = await _pickingReadService.GetPickingTaskFlat(
				TestDates.Today.AddDays(-2),
				TestDates.Today.AddDays(1),
				CancellationToken.None);

			// Assert
			var client = Assert.Single(result);
			Assert.Equal(11, client.ClientIdOut);

			var issue = Assert.Single(client.IssuesDetailsForPicking);
			Assert.Equal(IssueId, issue.IssueId);
			Assert.Equal(2, issue.IssueNumber);
			Assert.Equal(2, issue.Products.Count);

			var firstProduct = issue.Products.Single(product => product.ProductId == ProductId1);
			Assert.Equal("0987654321", firstProduct.SKU);
			Assert.Equal(100, firstProduct.Quantity);

			var secondProduct = issue.Products.Single(product => product.ProductId == ProductId2);
			Assert.Equal("fghtredfg", secondProduct.SKU);
			Assert.Equal(30, secondProduct.Quantity);
		}

		[Fact]
		public async Task GetPickingTaskFlat_ShouldReturnEmptyList_WhenDatesDoNotMatch()
		{
			// Act
			var result = await _pickingReadService.GetPickingTaskFlat(
				TestDates.Today.AddYears(1),
				TestDates.Today.AddYears(1).AddDays(1),
				CancellationToken.None);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetProductToIssueList_ShouldReturnAggregatedProducts_WhenTasksExist()
		{
			// Act
			var result = await _pickingReadService.GetProductToIssueList(
				TestDates.Today.AddDays(-2),
				TestDates.Today.AddDays(1),
				CancellationToken.None);

			// Assert
			Assert.Equal(2, result.Count);

			var firstProduct = result.Single(product => product.ProductId == ProductId1);
			Assert.Equal(11, firstProduct.ClientIdOut);
			Assert.Equal(IssueId, firstProduct.IssueId);
			Assert.Equal(2, firstProduct.IssueNumber);
			Assert.Equal("0987654321", firstProduct.SKU);
			Assert.Equal(100, firstProduct.Quantity);

			var secondProduct = result.Single(product => product.ProductId == ProductId2);
			Assert.Equal("fghtredfg", secondProduct.SKU);
			Assert.Equal(30, secondProduct.Quantity);
		}

		[Fact]
		public async Task GetProductToIssueList_ShouldReturnEmptyList_WhenDatesDoNotMatch()
		{
			// Act
			var result = await _pickingReadService.GetProductToIssueList(
				TestDates.Today.AddYears(1),
				TestDates.Today.AddYears(1).AddDays(1),
				CancellationToken.None);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetSourcePalletList_ShouldReturnPagedPallets_WhenTasksExist()
		{
			// Act
			var result = await _pickingReadService.GetSourcePalletList(
				TestDates.Today.AddDays(-2),
				TestDates.Today.AddDays(1),
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(2, result.Items.Count);

			var firstPallet = result.Items.Single(pallet => pallet.PalletId == PalletIdWithPickingTasks);
			Assert.Equal("Q1100", firstPallet.PalletNumber);
			Assert.Equal(3, firstPallet.LocationId);
			Assert.Equal("3-24-4-5", firstPallet.LocationName);

			var secondPallet = result.Items.Single(pallet => pallet.PalletId == SecondPalletIdWithPickingTasks);
			Assert.Equal("Q1200", secondPallet.PalletNumber);
		}

		[Fact]
		public async Task GetSourcePalletList_ShouldReturnEmptyPage_WhenDatesDoNotMatch()
		{
			// Act
			var result = await _pickingReadService.GetSourcePalletList(
				TestDates.Today.AddYears(1),
				TestDates.Today.AddYears(1).AddDays(1),
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		[Fact]
		public async Task GetProperpickingTask_ShouldReturnIssueOption_WhenTasksExist()
		{
			// Act
			var result = await _pickingReadService.GetProperpickingTask(
				ProductId2,
				TestDates.Today,
				TestDates.Today.AddDays(1),
				CancellationToken.None);

			// Assert
			var issue = Assert.Single(result);
			Assert.Equal(IssueId, issue.IssueId);
			Assert.Equal(2, issue.IssueNumber);
			Assert.Equal(80, issue.QuantityToDo);
		}

		[Fact]
		public async Task GetProperpickingTask_ShouldReturnEmptyList_WhenProductHasNoTasks()
		{
			// Act
			var result = await _pickingReadService.GetProperpickingTask(
				ProductWithoutPickingTasksId,
				TestDates.Today,
				TestDates.Today.AddDays(1),
				CancellationToken.None);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetPickingTaskForPallet_ShouldReturnPagedAllocatedTasks_WhenTasksExist()
		{
			// Act
			var result = await _pickingReadService.GetPickingTaskForPallet(
				PalletIdWithPickingTasks,
				TestDates.Today,
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(2, result.Items.Count);
			Assert.All(result.Items, task =>
			{
				Assert.Equal(PalletIdWithPickingTasks, task.SourcePalletId);
				Assert.Equal("Q1100", task.SourcePalletNumber);
				Assert.Equal(IssueId, task.IssueId);
				Assert.Equal(2, task.IssueNumber);
				Assert.Equal(ProductId2, task.ProductId);
				Assert.Equal("fghtredfg", task.SKU);
				Assert.Equal(PickingStatus.Allocated, task.PickingStatus);
			});
			Assert.Equal(30, result.Items.Sum(task => task.RequestedQuantity));
		}

		[Fact]
		public async Task GetPickingTaskForPallet_ShouldReturnEmptyPage_WhenPalletHasNoVirtualPallet()
		{
			// Act
			var result = await _pickingReadService.GetPickingTaskForPallet(
				PalletIdWithoutVirtualPallet,
				TestDates.Today,
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}
	}
}
