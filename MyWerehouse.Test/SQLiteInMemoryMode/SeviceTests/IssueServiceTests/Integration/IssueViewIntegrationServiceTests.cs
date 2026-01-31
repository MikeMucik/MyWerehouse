using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
			var query = new GetIssueByIdQuery(2);

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(11, result.ClientId);
			Assert.Equal("U002", result.PerformedBy);
			Assert.Equal(DateTime.UtcNow.AddHours(23), result.IssueDateTimeSend, precision :TimeSpan.FromMinutes(1));

			Assert.NotNull(result.Pallets);
			Assert.Equal(3, result.Pallets.Count);
		}
		[Fact]
		public async Task GetIssueProductSummaryById_GetData_ReturnDTO()
		{
			//Arrange&Act
			var query = new GetIssueProductSummaryByIdQuery(2);

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(11, result.ClientId);
			Assert.Equal(2, result.Items.Count);
			Assert.Equal("U002", result.PerformedBy);
			Assert.Equal(DateTime.UtcNow.AddHours(23), result.DateToSend,  precision: TimeSpan.FromMinutes(1));

			Assert.NotNull(result.Items);
			Assert.Equal(400, result.Items.FirstOrDefault(x => x.ProductId == 11).Quantity);
			Assert.Equal(150, result.Items.FirstOrDefault(x => x.ProductId == 10).Quantity);
		}
		[Fact]
		public async Task LoadingIssueAsync_ProperData_ReturnList()
		{
			//Arrange&Act
			var query = new LoadingIssueListQuery(2, "user");
			var result = await _mediator.Send(query);
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
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), prod10_Q1000.BestBefore);

			// produkt 11
			var prod11_Q1000 = palletQ1000.ProductOnPalletIssue.First(p => p.ProductId == 11);
			Assert.Equal(200, prod11_Q1000.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), prod11_Q1000.BestBefore);

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
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), prod11_Q2000.BestBefore);
		}
	}
}
