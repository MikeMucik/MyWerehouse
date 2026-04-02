using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class PalletChangeLocationIntegrationServiceTests : TestBase// PalletIntegrationCommandService
	{
		[Fact]
		public async Task ChangeLocation_ChangeLocationPalletAsync_ChangeData()
		{
			//Arange	
			var category = new Category
			{
				Id =1,
				Name = "TestC"
			};
			var product1 = Product.Create("TestP", "qwerty123", 1, 100);
			
			var product2 = Product.Create("TestP", "qwerty456", 1, 100);
			
			var location1 = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			//var productOnPallet1 = new ProductOnPallet
			//{
			//	//PalletId = "Q2000",
			//	Product = product1,
			//	Quantity = 100,
			//	DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};
			//var productOnPallet2 = new ProductOnPallet
			//{
			//	//PalletId = "Q2000",
			//	Product = product2,
			//	Quantity = 200,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 4)
			//};
			var pallet = Pallet.CreateForTests("Q2000", new DateTime(2020, 1, 1, 0, 0, 0), 1, PalletStatus.Available, null, null);
			pallet.AddProductForTests(product1.Id, 100, new DateTime(2025, 4, 4, 8, 8, 8), new DateOnly(2027, 3, 3));
			pallet.AddProduct(product2.Id, 10, new DateOnly(2027, 3, 4));
			//var pallet = new Pallet
			//{
			//	PalletNumber = "Q2000",
			//	DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
			//	Location = location1,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> { productOnPallet1, productOnPallet2 }
			//};			
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);			
			DbContext.Locations.AddRange(location1, location2);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();

			var movement = new PalletMovement
			{
				DestinationLocationId = location1.Id,
				MovementDate = DateTime.UtcNow.AddDays(-2),
				PalletId = pallet.Id,
				PalletNumber = pallet.PalletNumber,
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movement2 = new PalletMovement
			{
				SourceLocationId = location1.Id,
				DestinationLocationId = location2.Id,
				MovementDate = DateTime.UtcNow.AddDays(-1),
				PalletId = pallet.Id,
				PalletNumber = pallet.PalletNumber,
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movementDetails1 = new PalletMovementDetail
			{
				PalletMovement = movement,
				ProductId = product1.Id,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				PalletMovement = movement2,
				ProductId = product1.Id,
				Quantity = 1,
			};
			DbContext.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			DbContext.PalletMovements.AddRange(movement, movement2);
			DbContext.SaveChanges();
			//Act
			var palletId = pallet.Id;
			var destinationLocation = 2;
			var userId = "U001";
			//var result = await _palletService.ChangeLocationPalletAsync(palletId, destinationLocation, userId);
			var result = await Mediator.Send(new ChangeLocationPalletCommand(palletId, destinationLocation, userId));
			//Assert
			Assert.True(result.IsSuccess);
			Assert.False(result.Result.RequiresConfirmation);
			Assert.Contains($"Paleta {pallet.Id} została umieszczona w lokalizacji. ", result.Message);
			var resultPallet = DbContext.Pallets.First(x => x.Id == palletId);
			Assert.Equal(location2.Id, resultPallet.LocationId);			

			var moments = DbContext.PalletMovements.Where(a=>a.PalletId == palletId)
				.OrderByDescending(a=>a.MovementDate)
				.ToList();
			Assert.Equal(3, moments.Count);
			var lastMovement = moments.First();
			Assert.Equal(destinationLocation, lastMovement.DestinationLocationId);
			Assert.Equal(userId, lastMovement.PerformedBy);
		}
		[Fact]
		public async Task LocationOccupied_ChangeLocationPalletAsync_GiveBackInformation()
		{
			//Arange			
			var category = new Category
			{Id = 1,
				Name = "TestC"
			};
			var product1 = Product.Create("TestP", "qwerty123", 1, 100);
			//var product1 = new Product
			//{
			//	Name = "TestP",
			//	SKU = "qwert123",
			//	AddedItemAd = new DateTime(2024,1,1),				
			//	Category = category,
			//	IsDeleted = false,
			//	CartonsPerPallet = 100,
			//};
			var product2 = Product.Create("TestP", "qwerty456", 1, 100);
			//var product2 = new Product
			//{
			//	SKU = "qwert456",
			//	CartonsPerPallet = 100,
			//	Category = category,
			//	Name = "TestP",
			//};
			var location1 = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			//var productOnPallet1 = new ProductOnPallet
			//{
			//	//PalletId = "Q2000",
			//	Product = product1,
			//	Quantity = 100,
			//	DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};
			//var productOnPallet2 = new ProductOnPallet
			//{
			//	//PalletId = "Q2001",
			//	Product = product2,
			//	Quantity = 200,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 4)
			//};
			var pallet1 = Pallet.CreateForTests("Q2000", new DateTime(2020, 1, 1, 0, 0, 0), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 4, 4, 8, 8, 8), new DateOnly(2027, 3, 3));

			//var pallet1 = new Pallet
			//{
			//	PalletNumber = "Q2000",
			//	DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
			//	Location = location1,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> { productOnPallet1 }
			//};

			var pallet2 = Pallet.CreateForTests("Q2001", new DateTime(2020, 1, 1, 0, 0, 0), 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 4));
			//var pallet2 = new Pallet
			//{
			//	PalletNumber = "Q2001",
			//	DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
			//	Location = location2,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> { productOnPallet1 }
			//};


			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);

			DbContext.Locations.AddRange(location1, location2);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();

			var movement = new PalletMovement
			{
				DestinationLocationId = 1,
				MovementDate = DateTime.Now.AddDays(-2),
				PalletId = pallet1.Id,
				PalletNumber = pallet1.PalletNumber,
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movement2 = new PalletMovement
			{
				SourceLocationId = 1,
				DestinationLocationId = 2,
				MovementDate = DateTime.Now.AddDays(-1),
				PalletId = pallet1.Id,
				PalletNumber = pallet1.PalletNumber,
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movementDetails1 = new PalletMovementDetail
			{
				PalletMovement = movement,
				ProductId = product1.Id,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				PalletMovement = movement2,
				ProductId = product1.Id,
				Quantity = 1,
			};
			DbContext.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			DbContext.PalletMovements.AddRange(movement, movement2);
			DbContext.SaveChanges();
			//Act
			var palletId = pallet1.Id;
			var destinationLocation = 2;
			var userId = "U001";
			var result = await Mediator.Send(new ChangeLocationPalletCommand(palletId, destinationLocation, userId));
			
			//Assert
			Assert.True(result.IsSuccess);
			Assert.False(result.Result.Success);
			Assert.True(result.Result.RequiresConfirmation);
			var fullNameLocation = $" Bay = {location2.Bay} Aisle = {location2.Aisle} Position = {location2.Position} Height ={location2.Height}";
			Assert.Contains($"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {pallet2.Id}.", result.Message);
		}
		[Fact]
		public async Task LocationOccupied_ChangeLocationPalletAsync_GiveBackInformationAndPutAnotherPallet()
		{						
			//Arange			
			var category = new Category
			{
				Id =1,
				Name = "TestC"
			};
			var product1 = Product.Create("TestP", "qwerty123", 1, 100);
			//var product1 = new Product
			//{
			//	Name = "TestP",
			//	SKU = "qwert123",
			//	AddedItemAd = new DateTime(2024,1,1),				
			//	Category = category,
			//	IsDeleted = false,
			//	CartonsPerPallet = 100,
			//};
			var product2 = Product.Create("TestP", "qwerty456", 1, 100);
			//var product2 = new Product
			//{
			//	SKU = "qwert456",
			//	CartonsPerPallet = 100,
			//	Category = category,
			//	Name = "TestP",
			//};
			var location1 = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			//var productOnPallet1 = new ProductOnPallet
			//{
			//	//PalletId = "Q2000",
			//	Product = product1,
			//	Quantity = 100,
			//	DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
			//	BestBefore = new DateOnly(2027, 3, 3)
			//};
			//var productOnPallet2 = new ProductOnPallet
			//{
			//	//PalletId = "Q2001",
			//	Product = product2,
			//	Quantity = 200,
			//	DateAdded = DateTime.Now,
			//	BestBefore = new DateOnly(2027, 3, 4)
			//};
			var pallet1 = Pallet.CreateForTests("Q2000", new DateTime(2020, 1, 1, 0, 0, 0), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 4, 4, 8, 8, 8), new DateOnly(2027, 3, 3));
			//var pallet1 = new Pallet
			//{
			//	PalletNumber = "Q2000",
			//	DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
			//	Location = location1,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> { productOnPallet1 }
			//};
			var pallet2 = Pallet.CreateForTests("Q2001", new DateTime(2020, 1, 1, 0, 0, 0), 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 4));
			//var pallet2 = new Pallet
			//{
			//	PalletNumber = "Q2001",
			//	DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
			//	Location = location2,
			//	Status = PalletStatus.Available,
			//	//ProductsOnPallet = new List<ProductOnPallet> { productOnPallet1 }//
			//	ProductsOnPallet = new List<ProductOnPallet> { productOnPallet2 }//
			//};

			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);

			DbContext.Locations.AddRange(location1, location2);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();

			var movement = new PalletMovement
			{
				DestinationLocationId = 1,
				MovementDate = DateTime.Now.AddDays(-2),
				PalletId = pallet1.Id,
				PalletNumber = pallet1.PalletNumber,
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movement2 = new PalletMovement
			{
				SourceLocationId = 1,
				DestinationLocationId = 2,
				MovementDate = DateTime.Now.AddDays(-1),
				PalletId = pallet1.Id,
				PalletNumber = pallet1.PalletNumber,
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movementDetails1 = new PalletMovementDetail
			{
				PalletMovement = movement,
				ProductId = product1.Id,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				PalletMovement = movement2,
				ProductId = product1.Id,
				Quantity = 1,
			};
			DbContext.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			DbContext.PalletMovements.AddRange(movement, movement2);
			DbContext.SaveChanges();
			//Act
			var palletId = pallet1.Id;
			var destinationLocation = 2;
			var userId = "U001";

			var result = await Mediator.Send(new ChangeLocationPalletCommand(palletId, destinationLocation, userId, true));
			//Assert
			Assert.True(result.IsSuccess);
			Assert.False(result.Result.RequiresConfirmation);
			Assert.Contains($"Paleta {pallet1.Id} została umieszczona w lokalizacji. ", result.Message);

			// sprawdzamy, że obie palety siedzą w tej samej lokalizacji
			var movedPallet = DbContext.Pallets.First(x => x.Id == palletId);
			var existingPallet = DbContext.Pallets.First(x => x.Id == pallet2.Id);

			Assert.Equal(destinationLocation, movedPallet.LocationId);
			Assert.Equal(destinationLocation, existingPallet.LocationId);

			// sprawdzamy, że ruch został zapisany poprawnie			
			var moments = DbContext.PalletMovements.Where(a => a.PalletId == palletId)
				.OrderByDescending(a => a.MovementDate)
				.ToList();
			var lastMovement = moments.First();
			Assert.Equal(destinationLocation, lastMovement.DestinationLocationId);
			Assert.Equal(userId, lastMovement.PerformedBy);
			Assert.Equal(ReasonMovement.Moved, lastMovement.Reason);
		}
	}
}
