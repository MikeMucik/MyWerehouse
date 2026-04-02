using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Application.Receipts.Commands.DeleteReceipt;
using MyWerehouse.Domain.DomainExceptions;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptCancelIntegrationTests : TestBase
	{
		//Zmiana metody z delete na cancel
		[Fact]
		public async Task DeleteReceiptAsync_NotVerifiedReceipt_CancelledInBase()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var product = Product.Create("Test", "666666", 1, 56);
		
			var product1 = Product.Create("Test", "666666", 1, 56);
			
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			initialPallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			//var initialPallet = new Pallet
			//{
			//	PalletNumber = "Q1000",
			//	DateReceived = DateTime.Now,
			//	LocationId = 1,
			//	Status = PalletStatus.Available,				
			//};
			var initialPallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			initialPallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			//var initialPallet1 = new Pallet
			//{
			//	PalletNumber = "Q2000",
			//	DateReceived = DateTime.Now,
			//	LocationId = 1,
			//	Status = PalletStatus.Available,				
			//};
			//var initialProductOnPallet = new ProductOnPallet
			//{
			//	Id = 1,
			//	Pallet = initialPallet,
			//	//PalletId = "Q1000",
			//	//ProductId = 10,
			//	Product = product,
			//	Quantity = 100,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};
			//var initialProductOnPallet1 = new ProductOnPallet
			//{
			//	Id = 2,
			//	Pallet = initialPallet1,
			//	//PalletId = "Q2000",
			//	//ProductId = 1,
			//	Product = product1,
			//	Quantity = 200,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			receipt.AttachPallet(initialPallet, initailLocation, "U001");
			receipt.AttachPallet(initialPallet1, initailLocation, "U001");

			
			//var initialReceipt = new Receipt
			//{
			//	Id = receiptId1,
			//	ReceiptNumber = 1,
			//	ClientId = 1,
			//	ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
			//	PerformedBy = "U002",
			//	ReceiptDateTime = new DateTime(2025, 6, 6),
			//	Pallets = [initialPallet, initialPallet1]
			//};
			
			
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(product, product1);
			//DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new DeleteReceiptCommand(receipt.Id, "user"));
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Anulowano przyjęcie wraz z paletami z bazy", result.Message);
			var receiptE = DbContext.Receipts.FirstOrDefault(receipt => receipt.Id == receipt.Id);
			Assert.NotNull(receiptE);
			Assert.Equal(ReceiptStatus.Cancelled, receiptE.ReceiptStatus);
			//Assert.Empty(DbContext.Receipts);
			Assert.NotNull(DbContext.Pallets);
			var pallets = DbContext.Pallets.Where(p=>p.ReceiptId == receipt.Id).ToList();
			foreach (var palet in pallets)
			{
				Assert.Equal(PalletStatus.Cancelled, palet.Status);
			}
			Assert.NotNull(DbContext.ProductOnPallet);

			Assert.Equal(2, DbContext.Products.Count());
			Assert.Equal(1, DbContext.Categories.Count());
			Assert.Equal(1, DbContext.Clients.Count());
		}
		[Fact]
		public async Task DeleteReceiptAsync_NotVerifiedReceipt_RemovedBase()
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
			var client = new Client
			{				
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};

			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			
			//var initialReceipt = new Receipt
			//{
			//	Id = Guid.NewGuid(),
			//	ReceiptNumber = 1,
			//	Client = client,
			//	ReceiptStatus = ReceiptStatus.Planned,
			//	PerformedBy = "U002",
			//	ReceiptDateTime = new DateTime(2025, 6, 6),
			//};			
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(initialReceipt);		
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new DeleteReceiptCommand(initialReceipt.Id, "user"));
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Anulowano przyjęcie z bazy", result.Message);
		}
			[Fact]
		public async Task DeleteReceiptAsync_VerifiedReceipt_ThrowInfo()
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialCategory = new Category
			{	Id =1,
				Name = "name",
				IsDeleted = false
			};
			var initialLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = Product.Create("Test", "666666", 1, 56);
			
			var initialProduct1 = Product.Create("Test", "666666", 1, 56);
			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Verified, 1);
			//var initialReceipt = new Receipt
			//{
			//	Id = Guid.NewGuid(),
			//	ReceiptNumber = 1,
			//	Client = initailCLient,
			//	ReceiptStatus = ReceiptStatus.Verified,
			//	PerformedBy = "U002",
			//	ReceiptDateTime = new DateTime(2025, 6, 6),				
			//};
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, initialReceipt.Id, null);
			initialPallet.AddProduct(initialProduct.Id, 100, new DateOnly(2027, 3, 3));
			//var initialPallet = new Pallet
			//{
			//	PalletNumber = "Q1000",
			//	DateReceived = DateTime.Now,
			//	Location = initialLocation,
			//	Status = PalletStatus.Available,
			//	Receipt = initialReceipt,
			//};
			var initialPallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, initialReceipt.Id, null);
			initialPallet1.AddProduct(initialProduct1.Id, 200, new DateOnly(2027, 3, 3));
			//var initialPallet1 = new Pallet
			//{
			//	PalletNumber = "Q2000",
			//	DateReceived = DateTime.Now,
			//	Location = initialLocation,
			//	Status = PalletStatus.Available,
			//	Receipt = initialReceipt,
			//};
			//var initialProductOnPallet = new ProductOnPallet
			//{				
			//	Pallet = initialPallet,
			//	//PalletId = "Q1000",
			//	Product = initialProduct,
			//	Quantity = 100,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};
			//var initialProductOnPallet1 = new ProductOnPallet
			//{		
			//	Pallet = initialPallet1,
			//	//PalletId = "Q2000",
			//	Product = initialProduct1,
			//	Quantity = 200,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};	
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			//DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert		
			//var result = await Mediator.Send(new DeleteReceiptCommand(initialReceipt.Id, "user"));
			var ex = await Assert.ThrowsAsync<DomainReceiptException>(async()=> await Mediator.Send(new DeleteReceiptCommand(initialReceipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains("Nie można usunąć zweryfikowanego przyjęcia", ex.Message);		
		}
	}
}
