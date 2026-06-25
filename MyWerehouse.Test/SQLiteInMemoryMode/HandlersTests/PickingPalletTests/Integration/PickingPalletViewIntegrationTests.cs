using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree;
using MyWerehouse.Application.Picking.Queries.GetListPickingPallet;
using MyWerehouse.Application.Picking.Queries.GetListPickingPalletForOperator;
using MyWerehouse.Application.Picking.Queries.GetListToPickingFlat;
using MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking;
using MyWerehouse.Application.Picking.Queries.ShowTaskToDo;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PickingPalletTests.Integration
{
	[Collection("QueryCollection")]
	public class PickingPalletViewIntegrationTests
	{
		private readonly QueryTestSQLFixture _fixture;
		private readonly IMediator _mediator;
		public PickingPalletViewIntegrationTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_mediator = _fixture.Mediator;
		}
		[Fact]
		public async Task PrepareEmergencyPicking_ReturnValidInfo_WhenPalletHasOneProduct()
		{
			// Arrange
			var palletGuid8 = Guid.Parse("00000000-0008-1111-0000-000000000000");
			var today = DateOnly.FromDateTime(DateTime.UtcNow);
			var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
			var query = new PrepareEmergencyPickingQuery(palletGuid8, today, tomorrow);
			// Act			
			var result = await _mediator.Send(query);
			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal("Podaj numer zamówienia by kontynuować", result.Result.Message);
			Assert.NotNull(result.Result.IssueOptions);
			Assert.Single(result.Result.IssueOptions);
		}
		[Fact]
		public async Task PrepareEmergencyPicking_ReturnInfoDifferentProducts_WhenPalletWithManyProducts()
		{
			// Arrange
			var palletGuid9 = Guid.Parse("00000000-0009-1111-0000-000000000000");		
			var query = new PrepareEmergencyPickingQuery(palletGuid9, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(0)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));			
			// Act			
			var result = await _mediator.Send(query);
			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("nie jest do pickingu, zawiera rózne towary", result.Error);
		}
		[Fact]
		public async Task PrepareEmergencyPicking_ReturnInfoChangeStatus_WhenPalletHasWrongStatus()
		{
			// Arrange
			var palletGuid7 = Guid.Parse("00000000-0007-1111-0000-000000000000");
			var query = new PrepareEmergencyPickingQuery(palletGuid7, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(0)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
			// Act			
			var result = await _mediator.Send(query);
			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("Paleta nie jest w pickingu, zmień status.", result.Error);
		}
		[Fact]
		public async Task ShowTaskToDo_ShouldReturnPagedPickingTasks_WhenProperPallet()
		{
			// Arrange
			var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");
			var pallet = palletGuid5;
			var today =DateOnly.FromDateTime( DateTime.UtcNow);
			var query = new ShowTaskToDoQuery(pallet, today,1,1);
			// Act
			var result = await _mediator.Send(query);
			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.IsType<PagedResult<PickingTaskDTO>>(result.Result);
			Assert.NotEmpty(result.Result.Items);
		}
		[Fact]
		public async Task GetListToPicking_ReturnListForPickingTask()
		{
			// wytyczne - lista ile jakiego produktu do konkretnego zlecenia -zlecenia na daną chwilę, bierzemy zlecenia z danego okresu / dnia
			// pojedyncze rekordy dla każdej alokacji
			// Arrange
			var query = new GetListToPickingQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
			// Act
			var result = await _mediator.Send(query);
			// Assert 
			Assert.NotNull(result.Result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);			
			Assert.IsType<List<ProductToIssueDTO>>(result.Result);
			Assert.Equal(2, result.Result.Count);
		}	
		[Fact]
		public async Task GetListPickingPallet_ReturnListOfPallets()
		{
			//lista palet do zdjęcia przez wózkowego pallet's list for operator
			// Arrange
			var query = new GetListPickingPalletQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 1,5);
			// Act
			var result = await _mediator.Send(query);
			// Assert 
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Items);
			Assert.IsType<AppResult<PagedResult<PickingPalletWithLocationDTO>>>(result);
			Assert.Equal(2, result.Result.Items.Count);
		}
		[Fact]
		public async Task GetListIssueToPicking_ReturnListByClientAndIssue()
		{
			//Lista ile danego towaru dla danego zlecenia posegregowane i zgrupowane po kliencie Product's list by issue&client
			// Arrange
			var query = new GetListIssueToPickingQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
			// Act
			var result = await _mediator.Send(query);
			// Assert 
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.IsType<AppResult<List<PickingGuideLineDTO>>>(result);
			Assert.Single(result.Result);
		}
	}
}
