using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptCompletePhysicalIntegrationService : ReceiptIntegratioCommandService
	{
		[Fact]
		public async Task VerifyAndFinalizeReceiptAsync_WhenValid_UpdatesStatusAndInventory()
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
			var client = new Client
			{				
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var category = new Category
			{				
				Name = "Category A",
				IsDeleted = false
			};
			var product = new Product
			{				
				Name = "Product A",
				SKU = "123456",
				Category = category,
				IsDeleted = false
			};
			var location = new Location
			{				
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = new Pallet
			{
				Id = "PAL001",
				Location = location,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
						{				
				Product = product,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var pallet1 = new Pallet
			{
				Id = "PAL002",
				Location = location,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet			{				
				Product = product,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var receipt = new Receipt
			{				
				Client = client,
				ReceiptStatus = ReceiptStatus.InProgress,
				PerformedBy = "U001",
				Pallets = [pallet, pallet1]
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await _receiptService.CompletePhysicalReceiptAsync(receipt.Id, "user");
			//Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(ReceiptStatus.PhysicallyCompleted, receipt.ReceiptStatus);
		}
		[Fact]
		public async Task VerifyAndFinalizeReceiptAsync_WhenInValid_DontUpdatesStatusAndInventory()
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
			var client = new Client
			{

				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var category = new Category
			{
				Name = "Category A",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Product A",
				SKU = "123456",
				Category = category,
				IsDeleted = false
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = new Pallet
			{
				Id = "PAL001",
				Location = location,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
						{
				Product = product,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var pallet1 = new Pallet
			{
				Id = "PAL002",
				Location = location,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet            {
				Product = product,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var receipt = new Receipt
			{
				Client = client,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U001",
				Pallets = [pallet, pallet1]
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await _receiptService.CompletePhysicalReceiptAsync(receipt.Id, "user");
			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
		}
	}
}