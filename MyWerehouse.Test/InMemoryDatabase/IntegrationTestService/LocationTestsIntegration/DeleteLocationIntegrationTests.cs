using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.LocationTestsIntegration
{
	public class DeleteLocationIntegrationTests : LocationIntegrationCommand
	{
		private static Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "Cat",
				IsDeleted = false
			};
		}
		private static Product CreateProduct(string name)
		{
			return Product.Create(name, "SKU1", 1, 10);
		}
		[Fact]
		public async Task DeleteLocation_ShouldRemoveLocation_WhenPlaceIsEmpty()
		{
			//Arrange
			var locationDTO = new LocationDTO
			{
				Id =1,
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1,
			};
			//Act 1
			var result = await _locationService.AddLocationServiceAsync(locationDTO);
			//Assert 1
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal(1, result.Result);
			Assert.Contains("Dodano lokalizacje.", result.Message);
			var locationId = 1;
			//Act
			var resultDeleting = await _locationService.DeleteLocationServiceAsync(locationId);
			//Assert
			Assert.NotNull(resultDeleting);
			Assert.True(resultDeleting.IsSuccess);
			Assert.Contains("Operacja zakończyła się sukcesem", resultDeleting.Message);
		}

		[Fact]
		public async Task DeleteLocation_ShouldReturnErrorInfo_WhenPlaceOccupied()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct("Prod1");
			var palletP1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			palletP1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			_context.Categories.Add(category);
			_context.Products.Add(product);
			_context.Pallets.AddRange(palletP1);
			await _context.SaveChangesAsync();
			var locationDTO = new LocationDTO
			{
				Id =1,
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1,
			};
			//Act 1
			var result = await _locationService.AddLocationServiceAsync(locationDTO);
			//Assert 1
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal(1, result.Result);
			Assert.Contains("Dodano lokalizacje.", result.Message);
			var locationId = 1;

			//Act
			var resultDeleting = await _locationService.DeleteLocationServiceAsync(locationId);
			//Assert
			Assert.NotNull(resultDeleting);
			Assert.False(resultDeleting.IsSuccess);
			Assert.Contains("Miejsce paletowe nie jest puste nie można skasować", resultDeleting.Error);
		}
	}
}
