using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PickingPalletTestsRepoSQLite
{
	public class AddETCPickingProductOnPalletTests : TestBase
	{
		[Fact]
		public void AddNewRecord_AddPalletToPicking_AddToCollectionVirtualPallet()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var product = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Locations.Add(location);

			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> {
					new ProductOnPallet
					{
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddMonths(-1),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
					}
				}
			};
			DbContext.Pallets.Add(pallet);
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet,
				LocationId = pallet.LocationId,
				InitialPalletQuantity = pallet.ProductsOnPallet.First().Quantity,
				DateMoved = DateTime.Now,
				PickingTasks = new List<PickingTask>()

			};
			DbContext.SaveChanges();
			var pickingPalletRepo = new PickingPalletRepo(DbContext);
			//Act
			pickingPalletRepo.AddPalletToPicking(virtualPallet);
			DbContext.SaveChanges();
			//Assert
			var createdVirtualPallet = DbContext.VirtualPallets
				.Include(v => v.Pallet)
				.ThenInclude(p => p.ProductsOnPallet)
				.FirstOrDefault(v => v.Id == virtualPallet.Id);

			Assert.NotNull(createdVirtualPallet);
			Assert.Equal(virtualPallet.Id, createdVirtualPallet.Id);

			// Sprawdź relację z Pallet
			Assert.NotNull(createdVirtualPallet.Pallet);
			Assert.Equal("Q00001", createdVirtualPallet.Pallet.Id);
			Assert.Equal(pallet.LocationId, createdVirtualPallet.LocationId);

			// Sprawdź ilości
			Assert.Equal(10, createdVirtualPallet.InitialPalletQuantity);
			Assert.Empty(createdVirtualPallet.PickingTasks);

			// Sprawdź powiązany produkt
			var productOnPallet = createdVirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault();
			Assert.NotNull(productOnPallet);
			Assert.Equal("Banana", productOnPallet.Product.Name);
			Assert.Equal("1234567890", productOnPallet.Product.SKU);
			Assert.Equal(10, productOnPallet.Quantity);

			// Sprawdź, że VirtualPallet faktycznie trafił do kolekcji VirtualPallets w DbContext
			Assert.Contains(DbContext.VirtualPallets, v => v.Id == virtualPallet.Id);
		}
		[Fact]
		public void DeleteRecord_DeletePalletToPicking_ReomveFromCollectionVirtualPallet()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var product = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Locations.Add(location);
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> {
					new ProductOnPallet
					{
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddMonths(-1),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
					}
				}
			};
			DbContext.Pallets.Add(pallet);
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet,
				LocationId = pallet.LocationId,
				InitialPalletQuantity = pallet.ProductsOnPallet.First().Quantity,
				DateMoved = DateTime.Now,
				PickingTasks = new List<PickingTask>()

			};
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.SaveChanges();
			var pickingPalletRepo = new PickingPalletRepo(DbContext);
			//Act
			pickingPalletRepo.DeleteVirtualPalletPicking(virtualPallet);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.VirtualPallets.Find(virtualPallet.Id);
			Assert.Null(result);
		}
		[Fact]
		public void FinishPallet_ClosePickingPallet_ChangeStatusSetIssue()
		{
			//Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			DbContext.Clients.Add(initailClient);
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var product = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Locations.Add(location);
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = initailClient,
				PerformedBy = "U002",
				IssueDateTimeCreate = new DateTime(2025, 5, 5),
				IssueDateTimeSend = new DateTime(2025, 5, 6),//zmiana 
			};
			DbContext.Issues.Add(issue);
			var pickingPallettoClose = new Pallet
			{
				Id = "Q3000",
				Location = location,
				Status = PalletStatus.Picking,
				ProductsOnPallet = new List<ProductOnPallet> {
				new ProductOnPallet{
					Product = product,
					Quantity = 20
					}
				}
			};
			DbContext.Pallets.Add(pickingPallettoClose);
			DbContext.SaveChanges();
			//var issueId = 1;
			var pickingPalletRepo = new PickingPalletRepo(DbContext);
			//Act
			pickingPalletRepo.ClosePickingPallet(pickingPallettoClose.Id, issue.Id);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Pallets.Find(pickingPallettoClose.Id);
			Assert.NotNull(result);
			Assert.Equal(PalletStatus.ToIssue, result.Status);
			Assert.Equal(issue.Id, result.IssueId);
		}
	}
}
