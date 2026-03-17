using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PalletsTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewPalletTests
	{
		private readonly PalletRepo _palletRepo;
		private readonly QueryTestFixture _fixture;
		public ViewPalletTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_palletRepo = new PalletRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task GetPallet_GetPalletByIdAsync_ReturnSimplyData()
		{
			//Arrange
			var paletId = "Q1000";
			//Act
			var result =await _palletRepo.GetPalletByIdAsync(paletId);
			
			//Assert
			Assert.NotNull(result);
			Assert.Equal(PalletStatus.Available, result.Status);//DbCOntextFactory
			Assert.Equal(1, result.LocationId);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			Assert.Equal(receiptId1, result.ReceiptId);
		}

		[Fact]
		public async Task GetPallet_GetPalletWithProductsAsync_ReturnPalletWithProduct()
		{
			//Arrange
			var paletId = "Q1000";
			//Act
			var result = await _palletRepo.GetPalletByIdAsync(paletId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(50, result.ProductsOnPallet.First(p => p.Id == 1).Quantity);//50 Db
			Assert.Equal(new DateTime(2024, 2, 2), result.ProductsOnPallet.First(p => p.Id == 1).DateAdded);
		}
		
		[Fact]
		public void SearchPallets_FindPalletsByProductId_ReturnList()
		{
			//Arrange
			var productId = new PalletSearchFilter
			{
				ProductId = 10
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(4, result.Count());
			Assert.Contains(result, p => p.Id == "Q1000");
		}

		[Fact]
		public void SearchPallets_FindPalletsByDateReceved_ReturnList()
		{
			//Arrange
			var productId = new PalletSearchFilter
			{
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 3,3)
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(7, result.Count());
			Assert.Contains(result, p => p.Id == "Q1000");
		}
		[Fact]
		public void SearchPallets_FindPalletsByDateBestBefore_ReturnList()
		{
			//Arrange
			var productId = new PalletSearchFilter
			{
				BestBefore = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(9, result.Count());
			Assert.Contains(result, p => p.Id == "Q1000");
			Assert.Contains(result, p => p.Id == "Q1010");
		}
		[Fact]
		public void SearchPallets_FindPalletsByLocationId_ReturnList()
		{
			//Arrange
			var locationId = new PalletSearchFilter
			{
				LocationId = 1
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(locationId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			Assert.Contains(result, p=>p.Id == "Q1001");
			Assert.Contains(result, p=>p.Id == "Q1000");
		}
		[Fact]
		public void SearchPallets_FindPalletsByLocationId_ReturnNullList()
		{
			//Arrange
			var locationId = new PalletSearchFilter
			{
				LocationId = 2
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(locationId);
			//Assert
			Assert.Equal(0, result.Count());			
		}
		[Fact]
		public void SearchPalletsReceipt_GetPalletsByFilter_ReturnList()
		{
			//Arrange
			var clientId = new PalletSearchFilter
			{
				ClientIdIn = 10
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(clientId).ToList();
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count);

			foreach (var pallet in result)
			{
				Assert.NotNull(pallet.Receipt); 
				Assert.Equal(10, pallet.Receipt.ClientId);
				Assert.Contains(result, p => p.Id == "Q1000");
				Assert.Contains(result, p => p.Id == "Q1001");
				Assert.DoesNotContain(result, p => p.Id == "Q1010"); 				
				Assert.DoesNotContain(result, p => p.Id == "Q1002"); 				
			}
		}
		[Fact]
		public void SearchPalletsIssue_GetPalletsByFilter_ReturnList()
		{
			//Arrange
			var clientId = new PalletSearchFilter
			{
				ClientIdOut = 11
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(clientId).ToList();
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result); // zakładamy, że dane testowe zawierają takie palety
			Assert.Contains(result, p => p.Id == "Q1001");
			Assert.Contains(result, p => p.Id == "Q1000");
			foreach (var pallet in result)
			{
				Assert.NotNull(pallet.Issue); // powinno być przypisane
				Assert.Equal(11, pallet.Issue.ClientId);
				Assert.Contains(result, p => p.Id == "Q1000");
				Assert.Contains(result, p => p.Id == "Q1001");
				Assert.DoesNotContain(result, p => p.Id == "Q1010");
				Assert.DoesNotContain(result, p => p.Id == "Q1002");
			}
		}
		[Fact]
		public void SearchPallets_GetPalletsByFilter_ReturnList()
		{
			//Arrange
			var userId = new PalletSearchFilter
			{
				ReceiptUser = "U001"
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(userId);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			foreach (var pallet in result)
			{
				Assert.NotNull(pallet.Receipt);			
				Assert.Equal("U001", pallet.Receipt.PerformedBy);
			}
		}
		[Fact]
		public void SearchPalletsStatus_GetPalletsByFilter_ReturnList()
		{
			//Arrange
			var status = new PalletSearchFilter
			{
				PalletStatus = PalletStatus.Damaged,
			};
			//Act
			var result = _palletRepo.GetPalletsByFilter(status);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			foreach (var pallet in result)
			{
				Assert.Equal("Q1010", pallet.Id);
			}
		}
		[Fact]
		public void ReturnPalletsByProductIdAndDate_GetAvailablePallets_ReturnList()
		{
			//Arrange
			var productId = 10;
			DateOnly date = new DateOnly(2024,2,2);
			//Act
			var result = _palletRepo.GetAvailablePallets(productId, date);
			//Assert
			Assert.NotNull(result);					
			Assert.Equal(1, result.Count());			
			Assert.Contains(result, p => p.Id == "Q1000");							
		}
		[Fact]
		public void ReturnPalletsByProductIdAndDate2_GetAvailablePallets_ReturnList()
		{
			//Arrange
			var productId = 11;
			DateOnly date = new DateOnly(2024, 2, 2);
			//Act
			var result = _palletRepo.GetAvailablePallets(productId, date);
			//Assert
			Assert.NotNull(result);						
			Assert.Equal(2, result.Count());
			Assert.Contains(result, p => p.Id == "Q1002");						
			Assert.Contains(result, p => p.Id == "Q1000");						
		}
		[Fact]
		public async Task ReturnPallets_GetPalletByLocationAsync_ReturnList()
		{
			//Arrange
			var location = 1;
			//Act 
			var result = await _palletRepo.CheckOccupancyAsync(location);
			//Assert
			Assert.NotNull(result);
			//Assert.Equal(2, result.Count());
			Assert.True(result.Id == "Q1000" || result.Id == "Q1001");
				//||(result.Id, "Q1000"));
			
		}
	}
}
