using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.IssueTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewIssueTests : CommandTestBase
	{
		private readonly IssueRepo _issueRepo;
		public ViewIssueTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_issueRepo = new IssueRepo(_context);
		}
		[Fact]
		public void ShowIssueById_GetIssueFilter_ReturnIssue()
		{
			//Arrange
			var id = 2;
			//Act
			var result = _issueRepo.GetIssueById(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal(11, result.ClientId);
		}
		[Fact]
		public async Task ShowIssueById_GetIssueFilterAsync_ReturnIssue()
		{
			//Arrange
			var id = 2;
			//Act
			var result = await _issueRepo.GetIssueByIdAsync(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal(11, result.ClientId);
		}
		[Fact]
		public void ShowListIssuesByClient_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = 11
			};
			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
		}
		[Fact]
		public void ShowListIssuesByProduct_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = 10
			};
			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
		}
		[Fact]
		public void ShowListIssuesDate_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				DateTimeEnd = new DateTime(2025, 6, 6),
				DateTimeStart = new DateTime(2020, 1, 1),
			};

			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1001"));
		}
		//[Fact]
		//public void ShowAvailablePallets_GetAvailablePallets_ReturnList()
		//{
		//	//Arrange
		//	var productId = 10;
		//	var minBestBefore = new DateOnly(2026, 1, 1);
		//	//Act
		//	var result = _issueRepo.GetAvailablePallets(productId, minBestBefore);
		//	//Assert
		//	Assert.NotEmpty(result);
		//	Assert.Equal(1, result.Count());//1 -> DbFactory
		//}
		//[Fact]
		//public void TakePallets_SelectPalletsForIssue_ReturnList()
		//{
		//	//Arrange	
		//	var pallet1 = new Pallet
		//	{
		//		Id = "0001",
		//		ProductsOnPallet = new List<ProductOnPallet>
		//		{new ProductOnPallet { Quantity = 3 }
		//	},
		//		Status = PalletStatus.Available
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "0002",
		//		ProductsOnPallet = new List<ProductOnPallet>
		//		{  new ProductOnPallet { Quantity = 2 }
		//	},
		//		Status = PalletStatus.Available
		//	};
		//	var pallets = new List<Pallet> { pallet1, pallet2 }.AsQueryable();
		//	var quantity = 4;
		//	//Act
		//	var result = _issueRepo.SelectPalletsForIssue(pallets, quantity);
		//	//Assert
		//	Assert.NotEmpty(result);
		//	Assert.Equal(2, result.Count);
		//	Assert.Equal(PalletStatus.ToPicking, result[1].Status);
		//}
		//[Fact]
		//public async Task TakePallets_SelectPalletsForIssueAsync_ReturnList()
		//{
		//	//Arrange	
		//	var pallet1 = new Pallet
		//	{
		//		Id = "0001",
		//		ProductsOnPallet = new List<ProductOnPallet>
		//		{new ProductOnPallet { Quantity = 3 }
		//	},
		//		Status = PalletStatus.Available
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "0002",
		//		ProductsOnPallet = new List<ProductOnPallet>
		//		{  new ProductOnPallet { Quantity = 2 }
		//	},
		//		Status = PalletStatus.Available
		//	};
		//	var pallets = new IQueryable<Pallet> { pallet1, pallet2 }
		//	//.AsQueryable()
		//	//.AsEnumerable().ToList();
		//	;
		//	var quantity = 4;
		//	//Act
		//	var result =await _issueRepo.SelectPalletsForIssueAsync(pallets, quantity);
		//	//Assert
		//	Assert.NotEmpty(result);
		//	Assert.Equal(2, result.Count);
		//	Assert.Equal(PalletStatus.ToPicking, result[1].Status);
		//}
	}
}
