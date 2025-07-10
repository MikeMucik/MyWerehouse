using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
//using MyWerehouse.Test.IntegrationTest.PalletTestsIntegration;

namespace MyWerehouse.Test.IntegrationTestService.PalletTestsIntegration
{
	public class ChangingDataPalletIntegrationTests : PalletIntergrationCommand
	{
		[Fact]
		public void ChangeLocation_ChangeLocationPallet_ChangeData()
		{
			//Arange			
			var product1 = new ProductOnPallet
			{
				//Id = 1,
				PalletId = "Q2000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var product2 = new ProductOnPallet
			{
				//Id = 2,
				PalletId = "Q2000",
				ProductId = 20,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 4)
			};
			var pallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location1 = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Id = 2,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var movement = new PalletMovement
			{
				Id = 1,
				//Quantity = 1,
				DestinationLocationId = 1,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				//ProductId = 1,
				Reason = ReasonMovement.ManualMove,
			};
			var movement2 = new PalletMovement
			{
				Id = 2,
				//Quantity = 1,
				SourceLocationId = 1,
				DestinationLocationId = 2,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				//ProductId = 1,
				Reason = ReasonMovement.ManualMove,
			};
			var movementDetails1 = new PalletMovementDetail
			{
				Id = 1,
				PalletMovementId = 1,
				ProductId = 1,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				Id = 2,
				PalletMovementId = 2,
				ProductId = 1,
				Quantity = 1,
			};
			_context.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			_context.PalletMovements.AddRange(movement, movement2);
			_context.Locations.AddRange(location1, location2);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q2000";
			var destinationLocation = 2;
			var userId = "U001";
			_palletService.ChangeLocationPallet(palletId, destinationLocation, userId);
			//Assert
			var result = _context.Pallets.First(x => x.Id == palletId);
			Assert.Equal(location2.Id, result.LocationId);
			Assert.Equal(3, result.PalletMovements.Count);
		}
		[Fact]
		public async Task ChangeLocation_ChangeLocationPalletAsync_ChangeData()
		{
			//Arange			
			var product1 = new ProductOnPallet
			{
				//Id = 1,
				PalletId = "Q2000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var product2 = new ProductOnPallet
			{
				//Id = 2,
				PalletId = "Q2000",
				ProductId = 20,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 4)
			};
			var pallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location1 = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Id = 2,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var movement = new PalletMovement
			{
				Id = 1,				
				DestinationLocationId = 1,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,				
				Reason = ReasonMovement.ManualMove,
			};
			var movement2 = new PalletMovement
			{
				Id = 2,				
				SourceLocationId = 1,
				DestinationLocationId = 2,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,				
				Reason = ReasonMovement.ManualMove,
			};
			var movementDetails1 = new PalletMovementDetail
			{
				Id = 1,
				PalletMovementId = 1,
				ProductId = 1,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				Id = 2,
				PalletMovementId = 2,
				ProductId = 1,
				Quantity = 1,
			};
			_context.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			_context.PalletMovements.AddRange(movement, movement2);
			_context.Locations.AddRange(location1, location2);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q2000";
			var destinationLocation = 2;
			var userId = "U001";
			await _palletService.ChangeLocationPalletAsync(palletId, destinationLocation, userId);
			//Assert
			var result = _context.Pallets.First(x => x.Id == palletId);
			Assert.Equal(location2.Id, result.LocationId);
			Assert.Equal(3, result.PalletMovements.Count);
		}
		[Fact]
		public void LocationOccupied_ChangeLocationPallet_ThrowException()
		{
			//Arange			
			var product1 = new ProductOnPallet
			{
				//Id = 1,
				PalletId = "Q2000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var product2 = new ProductOnPallet
			{
				//Id = 2,
				PalletId = "Q2000",
				ProductId = 20,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 4)
			};
			var pallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var pallet1 = new Pallet
			{
				Id = "Q2001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 2,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location1 = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Id = 2,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var movement = new PalletMovement
			{
				Id = 1,				
				DestinationLocationId = 1,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,				
				Reason = ReasonMovement.ManualMove,
			};
			var movement2 = new PalletMovement
			{
				Id = 2,				
				SourceLocationId = 1,
				DestinationLocationId = 2,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,				
				Reason = ReasonMovement.ManualMove,
			};
			var movementDetails1 = new PalletMovementDetail
			{
				Id = 1,
				PalletMovementId = 1,
				ProductId = 1,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				Id = 2,
				PalletMovementId = 2,
				ProductId = 1,
				Quantity = 1,
			};
			_context.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			_context.PalletMovements.AddRange(movement, movement2);
			_context.Locations.AddRange(location1, location2);
			_context.Pallets.AddRange(pallet, pallet1);
			_context.SaveChanges();
			//Act&Assert
			var palletId = "Q2000";
			var destinationLocation = 2;
			var userId = "U001";
			var ex = Assert.Throws<InvalidOperationException>(()=> _palletService.ChangeLocationPallet(palletId, destinationLocation, userId));
			Assert.Contains($"Lokalizacja {destinationLocation} jest już zajęta przez paletę {pallet1.Id}.", ex.Message);			
		}
		[Fact]
		public async Task LocationOccupied_ChangeLocationPalletAsync_ThrowException()
		{
			//Arange			
			var product1 = new ProductOnPallet
			{
				//Id = 1,
				PalletId = "Q2000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2025, 4, 4, 8, 8, 8),
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var product2 = new ProductOnPallet
			{
				//Id = 2,
				PalletId = "Q2000",
				ProductId = 20,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 4)
			};
			var pallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var pallet1 = new Pallet
			{
				Id = "Q2001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 2,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location1 = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location2 = new Location
			{
				Id = 2,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var movement = new PalletMovement
			{
				Id = 1,
				DestinationLocationId = 1,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				Reason = ReasonMovement.ManualMove,
			};
			var movement2 = new PalletMovement
			{
				Id = 2,
				SourceLocationId = 1,
				DestinationLocationId = 2,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				Reason = ReasonMovement.ManualMove,
			};
			var movementDetails1 = new PalletMovementDetail
			{
				Id = 1,
				PalletMovementId = 1,
				ProductId = 1,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetail
			{
				Id = 2,
				PalletMovementId = 2,
				ProductId = 1,
				Quantity = 1,
			};
			_context.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			_context.PalletMovements.AddRange(movement, movement2);
			_context.Locations.AddRange(location1, location2);
			_context.Pallets.AddRange(pallet, pallet1);
			_context.SaveChanges();
			//Act&Assert
			var palletId = "Q2000";
			var destinationLocation = 2;
			var userId = "U001";
			var ex =await Assert.ThrowsAsync<InvalidOperationException>(() => _palletService.ChangeLocationPalletAsync(palletId, destinationLocation, userId));
			Assert.Contains($"Lokalizacja {destinationLocation} jest już zajęta przez paletę {pallet1.Id}.", ex.Message);
		}
	}
}
