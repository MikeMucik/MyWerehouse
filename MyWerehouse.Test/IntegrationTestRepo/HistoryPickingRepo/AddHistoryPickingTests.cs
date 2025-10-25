using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.HistoryPickingRepo
{
	public class AddHistoryPickingTests :TestBase
	{
		//[Fact]
		//public async Task CreateHistoryPickingAsync_SavesHistory_WithAllocationAndIssue()
		//{
		//	// Arrange
		//	var issue = await DbContext.Issues.FirstAsync();
		//	var virtualPallet = await DbContext.VirtualPallets.FirstAsync();
		//	var allocation = new Allocation
		//	{
		//		Issue = issue,
		//		VirtualPallet = virtualPallet,
		//		Quantity = 10,
		//		PickingStatus = PickingStatus.Allocated
		//	};
		//	DbContext.Allocations.Add(allocation);
		//	await DbContext.SaveChangesAsync();

		//	var service = new HistoryPickingService(DbContext);

		//	// Act
		//	await service.CreateHistoryPickingAsync(virtualPallet, allocation, "user123", PickingStatus.Created);

		//	// Assert
		//	var history = await DbContext.HistoryPickings.FirstOrDefaultAsync();
		//	Assert.NotNull(history);
		//	Assert.Equal(allocation.Id, history.AllocationId);
		//	Assert.Equal(issue.Id, history.IssueId);
		//	Assert.Equal("user123", history.PerformedBy);
		//}
		//[Fact]
		//public async Task DeletingAllocation_DoesNotDeleteHistory()
		//{
		//	// Arrange
		//	var issue = await DbContext.Issues.FirstAsync();
		//	var virtualPallet = await DbContext.VirtualPallets.FirstAsync();
		//	var allocation = new Allocation
		//	{
		//		Issue = issue,
		//		VirtualPallet = virtualPallet,
		//		Quantity = 5,
		//		PickingStatus = PickingStatus.Allocated
		//	};
		//	DbContext.Allocations.Add(allocation);
		//	await DbContext.SaveChangesAsync();

		//	var history = new HistoryPicking
		//	{
		//		Allocation = allocation,
		//		Issue = issue,
		//		VirtualPallet = virtualPallet,
		//		ProductId = 1,
		//		QuantityAllocated = 5,
		//		QuantityPicked = 0,
		//		StatusBefore = PickingStatus.Created,
		//		StatusAfter = PickingStatus.Allocated,
		//		PerformedBy = "tester",
		//		DateTime = DateTime.UtcNow
		//	};
		//	DbContext.HistoryPickings.Add(history);
		//	await DbContext.SaveChangesAsync();

		//	// Act
		//	DbContext.Allocations.Remove(allocation);
		//	await DbContext.SaveChangesAsync();

		//	// Assert
		//	var stillExists = await DbContext.HistoryPickings.AnyAsync();
		//	Assert.True(stillExists); // historia nie powinna zniknąć
		//}
		//[Fact]
		//public async Task HistoryWithNullAllocationId_IsValid()
		//{
		//	// Arrange
		//	var issue = await DbContext.Issues.FirstAsync();
		//	var virtualPallet = await DbContext.VirtualPallets.FirstAsync();

		//	var history = new HistoryPicking
		//	{
		//		AllocationId = null,
		//		Issue = issue,
		//		VirtualPallet = virtualPallet,
		//		ProductId = 1,
		//		QuantityAllocated = 0,
		//		QuantityPicked = 0,
		//		StatusBefore = PickingStatus.Created,
		//		StatusAfter = PickingStatus.Allocated,
		//		PerformedBy = "system",
		//		DateTime = DateTime.UtcNow
		//	};
		//	DbContext.HistoryPickings.Add(history);

		//	// Act
		//	await DbContext.SaveChangesAsync();

		//	// Assert
		//	Assert.NotEqual(0, history.Id);
		//}

	}
}
