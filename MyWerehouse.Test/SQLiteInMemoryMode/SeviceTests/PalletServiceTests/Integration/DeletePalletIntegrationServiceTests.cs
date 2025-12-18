using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class DeletePalletIntegrationServiceTests : PalletIntegrationCommandService
	{
		[Fact]
		public async Task PalletWithNoHistory_DeletePalletAsync_RemovePalletFromList()
		{
			//Arrange
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{

				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet { Product =product, Quantity =10,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
				,DateAdded = DateTime.UtcNow,
				}]
			};

			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			var palletId = "Q1010";
			//Act
			await _palletService.DeletePalletAsync(palletId);
			//Assert
			var result = await _palletRepo.GetPalletByIdAsync(palletId);
			Assert.Null(result);
		}
		[Fact]
		public async Task PalletWithHistory_DeletePalletAsync_ThrowException()
		{
			//Arrange

			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var location1 = new Location
			{
				Aisle = 0,
				Bay =1,
				Height = 0,
				Position = 0
			};
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet { Product = product, Quantity = 10,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
				,DateAdded = DateTime.UtcNow,
				}]
			};
			var movement = new PalletMovement
			{				
				DestinationLocationId = location.Id,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,				
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
			};
			var movement2 = new PalletMovement
			{ 
				SourceLocationId = location.Id,
				DestinationLocationId = location1.Id,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,				
				Reason = ReasonMovement.Moved,
				PerformedBy = "TestUser",
				PalletMovementDetails = [new PalletMovementDetail
			{
				ProductId = product.Id,
				Quantity = 1,
			},new PalletMovementDetail
			{
				ProductId = product.Id,
				Quantity = 1,
			}]
			};
			//DbContext.Locations.AddRange
			DbContext.Pallets.Add(pallet);
			DbContext.PalletMovements.AddRange(movement, movement2);
			DbContext.SaveChanges();
			var palletId = "Q1000";
			//Act&Assert
			var ex = await Assert.ThrowsAsync<PalletException>(() => _palletService.DeletePalletAsync(palletId));

			Assert.Contains($"Palety o numerze {palletId} nie można usunąć", ex.Message);
		}
		[Fact]
		public async Task IncorrectID_DeletePalletAsync_ThrowException()
		{
			//Arrange
			var palletId = "1000";
			//Act&Assert
			var ex = await Assert.ThrowsAsync<PalletException>(() => _palletService.DeletePalletAsync(palletId));

			Assert.Contains("Nie ma palety o numerze", ex.Message);
		}
	}
}
