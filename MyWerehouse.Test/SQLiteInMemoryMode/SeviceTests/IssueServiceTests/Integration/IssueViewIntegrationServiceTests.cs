using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.LoadingIssueList;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	[Collection("QueryCollection")]
	public class IssueViewIntegrationServiceTests
	{		
		private readonly QueryTestFixture _fixture;
		private readonly IMediator _mediator;
		public IssueViewIntegrationServiceTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_mediator = _fixture.Mediator;			
		}
		[Fact]
		public async Task GetIssueById_GetData_ReturnDTO()
		{
			//Arrange&Act
			var issueId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var query = new GetIssueByIdQuery(issueId2);

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(11, result.Result.ClientId);
			Assert.Equal("U002", result.Result.PerformedBy);
			Assert.Equal(DateTime.UtcNow.AddHours(23), result.Result.IssueDateTimeSend, precision :TimeSpan.FromMinutes(1));

			Assert.NotNull(result.Result.Pallets);
			Assert.Equal(3, result.Result.Pallets.Count);
		}
		[Fact]
		public async Task GetIssueProductSummaryById_GetData_ReturnDTO()
		{
			//Arrange&Act
			var issueId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			var query = new GetIssueProductSummaryByIdQuery(issueId2);

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(11, result.Result.ClientId);
			Assert.Equal(2, result.Result.Items.Count);
			Assert.Equal("U002", result.Result.PerformedBy);
			Assert.Equal(DateTime.UtcNow.AddHours(23), result.Result.DateToSend,  precision: TimeSpan.FromMinutes(1));

			Assert.NotNull(result.Result.Items);
			Assert.Equal(400, result.Result	.Items.FirstOrDefault(x => x.ProductId == productId2).Quantity);
			Assert.Equal(150, result.Result.Items.FirstOrDefault(x => x.ProductId == productId1).Quantity);
		}
		[Fact]
		public async Task LoadingIssueAsync_ProperData_ReturnList()
		{
			//Arrange&Act
			var issueId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			var query = new LoadingIssueListQuery(issueId2);
			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<AppResult<ListPalletsToLoadDTO>>(result);
			Assert.IsType<ListPalletsToLoadDTO>(result.Result);
			Assert.Equal(issueId2, result.Result.IssueId);
			Assert.Equal(11, result.Result.ClientId);
			Assert.Equal("ClientTest1", result.Result.ClientName);

			Assert.NotNull(result.Result.Pallets);
			Assert.Equal(2, result.Result.Pallets.Count);
			var palletQ1000 = result.Result.Pallets.First(p => p.PalletNumber == "Q1000");
			Assert.Equal(PalletStatus.Available, palletQ1000.PalletStatus);
			Assert.Equal(1, palletQ1000.LocationId);
			Assert.False(string.IsNullOrWhiteSpace(palletQ1000.LocationName));

			// produkty na Q1000
			Assert.NotNull(palletQ1000.ProductOnPalletIssue);
			Assert.Equal(2, palletQ1000.ProductOnPalletIssue.Count);

			// produkt 10
			var prod10_Q1000 = palletQ1000.ProductOnPalletIssue.First(p => p.ProductId == productId1);
			Assert.Equal(50, prod10_Q1000.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), prod10_Q1000.BestBefore);

			// produkt 11
			var prod11_Q1000 = palletQ1000.ProductOnPalletIssue.First(p => p.ProductId == productId2);
			Assert.Equal(200, prod11_Q1000.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), prod11_Q1000.BestBefore);

			// --- Paleta Q2000 ---
			var palletQ2000 = result.Result.Pallets.First(p => p.PalletNumber == "Q2000");
			Assert.Equal(PalletStatus.ToIssue, palletQ2000.PalletStatus);
			Assert.Equal(3, palletQ2000.LocationId);
			Assert.False(string.IsNullOrWhiteSpace(palletQ2000.LocationName));

			// produkty na Q2000
			Assert.NotNull(palletQ2000.ProductOnPalletIssue);
			Assert.Single(palletQ2000.ProductOnPalletIssue);

			var prod11_Q2000 = palletQ2000.ProductOnPalletIssue.First();
			Assert.Equal(productId2, prod11_Q2000.ProductId);
			Assert.Equal(200, prod11_Q2000.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), prod11_Q2000.BestBefore);
		}
	}
}
