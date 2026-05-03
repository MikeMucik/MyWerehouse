using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet;
using MyWerehouse.Application.PickingPallets.Queries.GetListToPicking;
using MyWerehouse.Application.PickingPallets.Queries.PrepareCorrectedPicking;
using MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	[Collection("QueryCollection")]
	public class PickingPalletViewIntegrationTests
	{
		private readonly QueryTestFixture _fixture;
		private readonly IMediator _mediator;
		public PickingPalletViewIntegrationTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_mediator = _fixture.Mediator;
		}
		[Fact]
		public async Task PrepareCorrectedPicking_GoodData_ReturnList()
		{
			// Arrange
			var palletGuid9 = Guid.Parse("00000000-0009-1111-0000-000000000000");

			//var query = new PrepareCorrectedPickingQuery("Q5000");			
			var query = new PrepareCorrectedPickingQuery(palletGuid9);			
			// Act			
			var result = await _mediator.Send(query);
			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal("Podaj numer zamówienia by kontynuować", result.Result.Message);
			Assert.NotNull(result.Result.IssueOptions);
			Assert.Equal(1, result.Result.IssueOptions.Count);
			//Assert.Contains("20", result.ProductInfo);
		}
		[Fact]
		public async Task ShowTaskToDo_GoodData_ReturnList()
		{
			// Arrange
			var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");


			//var pallet = "Q1100";
			var pallet = palletGuid5;
			var date = DateTime.UtcNow;
			var query = new ShowTaskToDoQuery(pallet, date,1,1);
			// Act
			var result = await _mediator.Send(query);
			// Assert
			Assert.IsType<PagedResult<PickingTaskDTO>>(result.Result);
			Assert.NotNull(result);
			Assert.NotEmpty(result.Result.Dtos);
		}
		[Fact]
		public async Task GetListToPicking_GoodData_ReturnListForPickingTask()
		{
			// wytyczne - lista ile jakiego produktu do konkretnego zlecenia -zlecenia na daną chwilę, bierzemy zlecenia z danego okresu / dnia
		// pojedyncze rekordy dla każdej alokacji
		// Arrange
			var query = new GetListToPickingQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
			// Act
			var result = await _mediator.Send(query);
			// Assert 
			Assert.IsType<List<ProductToIssueDTO>>(result.Result);
			Assert.NotEmpty(result.Result);
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Count);
		}	
		[Fact]
		public async Task GetListPickingPallet_GoodData_ReturnListOfPallets()
		{
			//lista palet do zdjęcia przez wózkowego pallet's list for operator
			// Arrange
			var query = new GetListPickingPalletQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
			// Act
			var result = await _mediator.Send(query);
			// Assert 
			Assert.IsType<AppResult<List<PickingPalletWithLocationDTO>>>(result);
			Assert.NotEmpty(result.Result);
			Assert.NotNull(result);
			Assert.Equal(3, result.Result.Count);
		}
		[Fact]
		public async Task GetListIssueToPicking_GoodData_ReturnListByClientAndIssue()
		{
			//Lista ile danego towaru dla danego zlecenia posegregowane i zgrupowane po kliencie Product's list by issue&client
			// Arrange
			var query = new GetListIssueToPickingQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
			// Act
			var result = await _mediator.Send(query);
			// Assert 
			Assert.IsType<AppResult<List<PickingGuideLineDTO>>>(result);
			Assert.NotEmpty(result.Result);
			Assert.NotNull(result);
			Assert.Equal(1, result.Result.Count);
		}
	}
}
