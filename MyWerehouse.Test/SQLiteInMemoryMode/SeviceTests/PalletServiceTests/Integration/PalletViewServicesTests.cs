using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	[Collection("QueryCollection")]	
	public class PalletViewServicesTests
	{
		private readonly QueryTestFixture _fixture;
		private readonly IMediator _mediator;
		public PalletViewServicesTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_mediator = _fixture.Mediator;
		}
		[Fact]
		public async Task ShowDataToEdit_GetPalletToEditAsync_ReturnUpdatePalletDTO()
		{
			//Arrange
			var palletId = "Q1001";
			var query = new GetPalletToEditQuery(palletId);
			//Act
			var result = await _mediator.Send(query);
			Assert.NotNull(result);
			Assert.Single(result.ProductsOnPallet);
			Assert.Equal(1, result.LocationId);
			Assert.Equal(PalletStatus.OnHold, result.Status);
			Assert.Equal(1, result.ReceiptId);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			var product1 = result.ProductsOnPallet.Single(p => p.ProductId == 10);
			Assert.Equal(100, product1.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), product1.BestBefore);
		}

		[Fact]
		public async Task ShowDataToEdit_ShowPalletAsync_ReturnData()
		{
			//Arrange
			var palletId = "Q1000";
			var query = new GetPalletToEditQuery(palletId);
			//Act

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.ProductsOnPallet);
			Assert.Equal(2, result.ProductsOnPallet.Count);
			Assert.Equal(1, result.ReceiptId);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			var product1 = result.ProductsOnPallet.Single(p => p.ProductId == 10);
			Assert.Equal(50, product1.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), product1.BestBefore);
			var product2 = result.ProductsOnPallet.Single(p => p.ProductId == 11);
			Assert.Equal(200, product2.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), product2.BestBefore);
		}
		[Fact]
		public void ShowPallets_FindPallets_ReturnCollection()
		{
			//Arrange
			var filtr = new PalletSearchFilter
			{
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))
			};
			//Act
			var query = new FindPalletsByFiltrQuery(filtr);
			var result = _mediator.Send(query);
			//Assert
			Assert.NotEmpty(result.Result);
		}
		[Fact]
		public void ShowPallets_FindPallets_ReturnCollectionEmpty()
		{
			//Arrange
			var filtr = new PalletSearchFilter
			{
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(36))
			};
			//Act
			var query = new FindPalletsByFiltrQuery(filtr);
			var result = _mediator.Send(query);
			//Assert
			Assert.Empty(result.Result);
		}
	}
}
