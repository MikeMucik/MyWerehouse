using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Infrastructure.Persistence.ReadServices;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class ReversePickingServiceTests : TestBase
	{
		private static readonly Guid ReversePickingTaskId1 =
			Guid.Parse("99999999-1111-1111-1111-111111111111");
		private static readonly Guid ReversePickingTaskId2 =
			Guid.Parse("99999999-2222-2222-2222-222222222222");
		private static readonly Guid PickingTaskId1 =
			Guid.Parse("88888888-1111-1111-1111-111111111111");
		private static readonly Guid PickingTaskId2 =
			Guid.Parse("88888888-2222-2222-2222-222222222222");
		private static readonly Guid PickingPalletId =
			Guid.Parse("00000000-0009-1111-0000-000000000000");
		private static readonly Guid SourcePalletId =
			Guid.Parse("00000000-0008-1111-0000-000000000000");
		private static readonly Guid SourceVirtualPalletId =
			Guid.Parse("22222222-3333-2222-1111-111111111111");
		private static readonly Guid ProductId =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid IssueId =
			Guid.Parse("11111111-2111-1111-1111-111111111111");

		private readonly ReversePickingReadService _reversePickingReadService;

		public ReversePickingServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_reversePickingReadService = new ReversePickingReadService(DbContext);
		}

		[Fact]
		public async Task GetListPalletsToReversePicking_ShouldReturnList_WhenTasksExist()
		{
			// Act
			var result = await _reversePickingReadService.GetListPalletsToReversePicking(
				TestDates.Today.AddDays(-2),
				TestDates.Today.AddDays(1),
				CancellationToken.None);

			// Assert
			var pallet = Assert.Single(result);
			Assert.Equal(PickingPalletId, pallet.PalletId);
			Assert.Equal("Q5000", pallet.PalletNumber);
			Assert.Equal(3, pallet.LocationId);
			Assert.Equal("3-24-4-5", pallet.LocationName);
			Assert.Equal(PalletStatus.Picking, pallet.Status);
		}

		[Fact]
		public async Task GetListPalletsToReversePicking_ShouldReturnEmptyList_WhenDatesDoNotMatch()
		{
			// Act
			var result = await _reversePickingReadService.GetListPalletsToReversePicking(
				TestDates.Today.AddYears(1),
				TestDates.Today.AddYears(1).AddDays(1),
				CancellationToken.None);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetReversePicking_ShouldReturnProjection_WhenTaskExists()
		{
			// Arrange
			var dateMade = TestDates.Today.AddDays(10);
			await AddReversePickingAsync(
				ReversePickingTaskId1,
				PickingTaskId1,
				dateMade);

			// Act
			var result = await _reversePickingReadService.GetReversePicking(
				ReversePickingTaskId1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ReversePickingTaskId1, result.Id);
			Assert.Equal("Q5000", result.PickingPalletNumber);
			Assert.Equal("Q1200", result.SourcePalletNumber);
			Assert.Equal("0987654321", result.ProductSKU);
			Assert.Equal(TestDates.Today.AddDays(365), result.BestBefore);
			Assert.Equal(10, result.Quantity);
			Assert.Equal(ReversePickingStatus.Ongoing, result.Status);
		}

		[Fact]
		public async Task GetReversePicking_ShouldReturnNull_WhenTaskDoesNotExist()
		{
			// Act
			var result = await _reversePickingReadService.GetReversePicking(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetReversePickingsInDates_ShouldReturnPagedTasks_WhenTasksExist()
		{
			// Arrange
			var dateMade = TestDates.Today.AddDays(10);
			await AddReversePickingAsync(
				ReversePickingTaskId1,
				PickingTaskId1,
				dateMade);
			await AddReversePickingAsync(
				ReversePickingTaskId2,
				PickingTaskId2,
				dateMade);

			// Act
			var result = await _reversePickingReadService.GetReversePickingsInDates(
				dateMade,
				dateMade,
				pageNumber: 1,
				pageSize: 1,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(1, result.CurrentPage);
			Assert.Equal(1, result.PageSize);
			Assert.True(result.HasNext);

			var reversePicking = Assert.Single(result.Items);
			Assert.Equal(ReversePickingTaskId1, reversePicking.Id);
			Assert.Equal("Q5000", reversePicking.PickingPalletNumber);
			Assert.Equal("Q1200", reversePicking.SourcePalletNumber);
			Assert.Equal("0987654321", reversePicking.ProductSKU);
			Assert.Equal(TestDates.Today.AddDays(365), reversePicking.BestBefore);
			Assert.Equal(10, reversePicking.Quantity);
			Assert.Equal(ReversePickingStatus.Ongoing, reversePicking.Status);
		}

		[Fact]
		public async Task GetReversePickingsInDates_ShouldReturnEmptyPage_WhenDatesDoNotMatch()
		{
			// Act
			var result = await _reversePickingReadService.GetReversePickingsInDates(
				TestDates.Today.AddYears(1),
				TestDates.Today.AddYears(1).AddDays(1),
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		private async Task AddReversePickingAsync(
			Guid reversePickingTaskId,
			Guid pickingTaskId,
			DateOnly dateMade)
		{
			var bestBefore = TestDates.Today.AddDays(365);
			var pickingTask = PickingTask.CreateForSeed(
				pickingTaskId,
				SourceVirtualPalletId,
				IssueId,
				requestedQuantity: 10,
				PickingStatus.Picked,
				ProductId,
				bestBefore,
				PickingPalletId,
				dateMade,
				pickedQuantity: 10);
			var reversePickingTask = ReversePickingTask.CreateForSeed(
				reversePickingTaskId,
				PickingPalletId,
				SourcePalletId,
				ProductId,
				bestBefore,
				quantity: 10,
				pickingTaskId,
				"UserR",
				dateMade);

			DbContext.PickingTasks.Add(pickingTask);
			DbContext.ReversePickings.Add(reversePickingTask);
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();
		}
	}
}
