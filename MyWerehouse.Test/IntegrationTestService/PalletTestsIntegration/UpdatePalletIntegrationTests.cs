using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using Xunit.Sdk;

namespace MyWerehouse.Test.IntegrationTestService.PalletTestsIntegration
{
	public class UpdatePalletIntegrationTests : PalletIntergrationCommand
	{
		
		[Fact]
		public async Task ProperData_UpdatePalletAsync_ChangeData()
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
			var updatingPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			_context.Locations.Add(location);
			_context.Pallets.Add(updatingPallet);
			_context.SaveChanges();
			//Act
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = product1.Id,
					PalletId = "Q2000",
					ProductId = 10,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					Id = product2.Id,
					PalletId = "Q2000",
					ProductId = 20,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					//Id = 300,
					PalletId = "Q2000",
					ProductId = 30,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			await _palletService.UpdatePalletAsync(updatedPallet, "user");
			//Assert

			var result = _context.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == updatingPallet.Id);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			Assert.Equal(updatedPallet.ProductsOnPallet.Count, result.ProductsOnPallet.Count);
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.Id == product2.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.Id == product2.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
		}
		
		[Fact]
		public async Task NonProperDataProduct_UpdatePalletAsync_ThrowException()
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
			var updatingPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			_context.Locations.Add(location);
			_context.Pallets.Add(updatingPallet);
			_context.SaveChanges();
			//Act&Assert
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = product1.Id,
					PalletId = "Q2000",
					//ProductId = 10,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					Id = product2.Id,
					PalletId = "Q2000",
					ProductId = 20,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					//Id = 300,
					PalletId = "Q2000",
					ProductId = 30,
					Quantity = 0,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2024, 5, 4) })
					]
			};
			var ex =await Assert.ThrowsAsync<ValidationException>(() => _palletService.UpdatePalletAsync(updatedPallet, "user"));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
			Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
			Assert.Contains("Data do spożycia musi być późniejsza niż data dzisiejsza", ex.Message);
		}
		
		[Fact]
		public async Task NonProperDataPallet_UpdatePalletAsync_ThrowException()
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
			var updatingPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { product1, product2 }
			};
			var location = new Location
			{
				Id = 1,
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			_context.Locations.Add(location);
			_context.Pallets.Add(updatingPallet);
			_context.SaveChanges();
			//Act&Assert
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q2000",
				//DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				//LocationId = 1,
				//Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = product1.Id,
					PalletId = "Q2000",
					ProductId = 10,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					Id = product2.Id,
					PalletId = "Q2000",
					ProductId = 20,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					//Id = 300,
					PalletId = "Q2000",
					ProductId = 30,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			var ex =await Assert.ThrowsAsync<ValidationException>(() => _palletService.UpdatePalletAsync(updatedPallet, "user"));
			Assert.Contains("Paleta musi mieć status", ex.Message);
			Assert.Contains("Paleta musi mieć datę utworzenia", ex.Message);
			Assert.Contains("Paleta musi mieć lokalizację", ex.Message);
		}
	}
}
