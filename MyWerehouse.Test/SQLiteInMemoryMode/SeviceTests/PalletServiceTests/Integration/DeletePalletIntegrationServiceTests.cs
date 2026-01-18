using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

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
			var result = await _palletService.DeletePalletAsync(palletId, "UserD");
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Paleta została usunięta", result.Message);
			var palletDeleted = await DbContext.Pallets.FindAsync(palletId);
			Assert.Null(palletDeleted);
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
				Bay = 1,
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
			//Act
			var result = _palletService.DeletePalletAsync(palletId, "UserD");
			//Assert
			Assert.Contains($"Palety o numerze {palletId} nie można usunąć", result.Result.Message);
		}
		[Fact]
		public async Task IncorrectID_DeletePalletAsync_ThrowInfo()
		{
			//Arrange
			var palletId = "1000";
			//Act
			var result = _palletService.DeletePalletAsync(palletId, "UserD");
			//Assert
			Assert.Contains("Paleta o numerze 1000 nie istnieje.", result.Result.Message);
		}
	}
}
