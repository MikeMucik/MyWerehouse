using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Unit
{
	public class IssueServiceTests : TestBase
	{
		[Fact]
		public async Task FinishIssueNotCompleted_ShouldUpdateStatusesAndCreateMovementsAndHistory()
		{
			// Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailLocation = new Location
			{
				Id = 10,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initailLocation1 = new Location
			{
				Id = 20,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Id = 101,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var issueId = 1;
			var performedBy = "Janek";

			var loadedPallet = new Pallet
			{
				Id = "P1",
				Status = PalletStatus.Loaded,
				LocationId = 10,
				ProductsOnPallet = new List<ProductOnPallet>
		{
			new ProductOnPallet { ProductId = 101, Quantity = 5, }
		}
			};

			var notLoadedPallet = new Pallet
			{
				Id = "P2",
				Status = PalletStatus.OnHold,
				LocationId = 20,
				ProductsOnPallet = new List<ProductOnPallet>()
			};

			var issue = new Issue
			{
				Id = issueId,
				ClientId = initailCLient.Id,
				IssueDateTime = new DateTime(2025, 6,6,2,2,2),
				Pallets = new List<Pallet> { loadedPallet, notLoadedPallet }
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.AddRange(initailLocation, initailLocation1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			var mockMapper = new Mock<IMapper>();
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issueId))
						 .ReturnsAsync(issue);

			var palletMovementServiceMock = new Mock<IPalletMovementService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			
			var palletRepo = new Mock<IPalletRepo>();
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,				
				palletMovementServiceMock.Object,
				inventoryRepoMock.Object,
				palletRepo.Object
			);

			// Act
			await service.FinishIssueNotCompleted(issueId, performedBy);

			// Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(PalletStatus.Available, notLoadedPallet.Status);
			Assert.Null(notLoadedPallet.IssueId);

			palletMovementServiceMock.Verify(x =>
				x.CreateMovementAsync(notLoadedPallet, 20, ReasonMovement.Correction, performedBy, null), Times.Once);

			palletMovementServiceMock.Verify(x =>
				x.CreateMovementAsync(loadedPallet, 10, ReasonMovement.Loaded, performedBy, null), Times.Once);

			inventoryRepoMock.Verify(x =>
				x.DecreaseInventoryQuantityAsync(101, 5), Times.Once);

			palletMovementServiceMock.Verify(x =>
				x.CreateHistoryIssueAsync(issue, IssueStatus.IsShipped, performedBy, null), Times.Once);
					;
			var updatedIssue = await DbContext.Issues
				.Include(i=>i.Pallets)								
				.FirstOrDefaultAsync(i => i.Id == issueId);

			Assert.NotNull(updatedIssue);
			Assert.Equal(IssueStatus.IsShipped, updatedIssue.IssueStatus);

			// sprawdź czy P2 została usunięta z przypisania do zlecenia:
			var palletP2 = await DbContext.Pallets.FindAsync("P2");
			Assert.Equal(PalletStatus.Available, palletP2.Status);
			Assert.Null(palletP2.IssueId);
		}

	}
}
