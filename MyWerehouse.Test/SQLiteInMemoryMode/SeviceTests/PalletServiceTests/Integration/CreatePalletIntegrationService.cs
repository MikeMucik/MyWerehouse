using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class CreatePalletIntegrationService : PalletIntegrationCommandService
	{
		[Fact]
		public async Task PalletWithNoHistory_CreatePalletAsync_CreateToList()
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

			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var newPallet = new PalletDTO
			{
				ProductsOnPallet = new HashSet<ProductOnPalletDTO>{ new ProductOnPalletDTO
						{
							ProductId = product.Id,
							Quantity = 5,
						}					
				},
			};
			//Act
			var result = await _palletService.CreatePalletAsync(newPallet, "user");
			//Assert
			Assert.False(string.IsNullOrWhiteSpace(result)); // numer palety został zwrócony

			// weryfikacja, że paleta faktycznie została utworzona w bazie
			var palletInDb = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Id == result);

			Assert.NotNull(palletInDb);
			Assert.Equal(result, palletInDb.Id);
			
			Assert.Single(palletInDb.ProductsOnPallet);
			Assert.Equal(product.Id, palletInDb.ProductsOnPallet.First().ProductId);
			Assert.Equal(5, palletInDb.ProductsOnPallet.First().Quantity);

			var history = await DbContext.PalletMovements.Include(p=>p.PalletMovementDetails)
				.FirstOrDefaultAsync(p=>p.PalletId ==  result);
			Assert.NotNull(history);
			Assert.NotEmpty(history.PalletMovementDetails);
			Assert.Equal("user", history.PerformedBy);
		}
	}
}
