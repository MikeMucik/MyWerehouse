using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	[Collection("QueryCollection")]
	public class IssueLoadingIssueIntegrationService
	{
		private readonly IssueService _issueService;
		private readonly IssueRepo _issueRepo;
		private readonly QueryTestFixture _fixture;
		public IssueLoadingIssueIntegrationService(QueryTestFixture fixture)
		{
			_fixture = fixture;			
			_issueRepo = new IssueRepo(_fixture.DbContext);
			_issueService = new IssueService(_issueRepo);
		}
		[Fact]
		public async Task LoadingIssueAsync_ProperData_ReturnList()
		{
			//Arrange&Act
			var result = await _issueService.LoadingIssueListAsync(2, "user");
			//Assert
			Assert.NotNull(result);
			Assert.IsType<ListPalletsToLoadDTO>(result);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(11, result.ClientId);
			Assert.Equal("ClientTest1", result.ClientName);

			Assert.NotNull(result.Pallets);
			Assert.Equal(2, result.Pallets.Count);
			var palletQ1000 = result.Pallets.First(p => p.PalletId == "Q1000");
			Assert.Equal(PalletStatus.Available, palletQ1000.PalletStatus);
			Assert.Equal(1, palletQ1000.LocationId);
			Assert.False(string.IsNullOrWhiteSpace(palletQ1000.LocationName));

			// produkty na Q1000
			Assert.NotNull(palletQ1000.ProductOnPalletIssue);
			Assert.Equal(2, palletQ1000.ProductOnPalletIssue.Count);

			// produkt 10
			var prod10_Q1000 = palletQ1000.ProductOnPalletIssue.First(p => p.ProductId == 10);
			Assert.Equal(50, prod10_Q1000.Quantity);
			Assert.Equal(new DateOnly(2026, 2, 2), prod10_Q1000.BestBefore);

			// produkt 11
			var prod11_Q1000 = palletQ1000.ProductOnPalletIssue.First(p => p.ProductId == 11);
			Assert.Equal(200, prod11_Q1000.Quantity);
			Assert.Equal(new DateOnly(2025, 2, 2), prod11_Q1000.BestBefore);

			// --- Paleta Q2000 ---
			var palletQ2000 = result.Pallets.First(p => p.PalletId == "Q2000");
			Assert.Equal(PalletStatus.ToIssue, palletQ2000.PalletStatus);
			Assert.Equal(3, palletQ2000.LocationId);
			Assert.False(string.IsNullOrWhiteSpace(palletQ2000.LocationName));

			// produkty na Q2000
			Assert.NotNull(palletQ2000.ProductOnPalletIssue);
			Assert.Single(palletQ2000.ProductOnPalletIssue);

			var prod11_Q2000 = palletQ2000.ProductOnPalletIssue.First();
			Assert.Equal(11, prod11_Q2000.ProductId);
			Assert.Equal(200, prod11_Q2000.Quantity);
			Assert.Equal(new DateOnly(2026, 2, 2), prod11_Q2000.BestBefore);			
		}
	}
}
