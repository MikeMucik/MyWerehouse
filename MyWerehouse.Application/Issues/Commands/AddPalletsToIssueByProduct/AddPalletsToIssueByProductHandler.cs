using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.IssuesServices;
using MyWerehouse.Application.Pallets.Services;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.AddPalletsToIssueByProduct
{
	public class AddPalletsToIssueByProductHandler(WerehouseDbContext werehouseDbContext,
		IMediator mediator,
		IEventCollector eventCollector,
		IProductRepo productRepo,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IGetVirtualPalletsService getVirtualPalletsService,
		IGetProductCountService getProductCount,
		IGetNumberPalletsAndRestService getNumberPalletsAndRestService,
		IGetAvailablePalletsByProductService getAvailablePalletsByProductService,
		IAssignFullPalletToIssueService assignFullPalletToIssueService) : IRequestHandler<AddPalletsToIssueByProductCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IMediator _mediator = mediator;
		private readonly IEventCollector _eventCollector = eventCollector;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IGetVirtualPalletsService _getVirtualPalletsService = getVirtualPalletsService;
		private readonly IGetProductCountService _getProductCount = getProductCount;
		private readonly IGetNumberPalletsAndRestService _getNumberPalletsAndRestService = getNumberPalletsAndRestService;
		private readonly IGetAvailablePalletsByProductService _getAvailable = getAvailablePalletsByProductService;
		private readonly IAssignFullPalletToIssueService _assignFullPalletToIssueService = assignFullPalletToIssueService;
		public async Task<IssueResult> Handle(AddPalletsToIssueByProductCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
			var totalAvailable = 0;
			try
			{
				if (request.Issue.IssueStatus == IssueStatus.New) request.Issue.IssueStatus = IssueStatus.Pending;
				if (request.Issue.IssueStatus != IssueStatus.Pending && request.Issue.IssueStatus != IssueStatus.New)
				{
					throw new NotFoundIssueException("Błąd statusu zlecenia");
				}
				if (await _productRepo.IsExistProduct(request.Product.ProductId) == false) throw new NotFoundProductException(request.Product.ProductId);
				//1 dostepność towaru - ile jest opakowań z datą dłuższą niż BestBefore
				//totalAvailable = await _mediator.Send(new GetProductCountQuery(request.Product.ProductId, request.Product.BestBefore), ct);
				totalAvailable = await _getProductCount.GetProductCountAsync(request.Product.ProductId, request.Product.BestBefore);
				if (request.Product.Quantity > totalAvailable)
				{
					return IssueResult.Fail($"Nie wystarczająca ilości produktu o numerze {request.Product.ProductId}. Asortyment nie został dodany do zlecenia.",
						request.Product.ProductId,
					request.Product.Quantity,
					totalAvailable);
				}
				//2 Oblicz pełne palety i resztę  
				//var palletAmountFullResult = await _mediator.Send(new GetNumberPalletsAndRestQuery(request.Product.ProductId, request.Product.Quantity), ct);
				var palletAmountFullResult = await _getNumberPalletsAndRestService.GetNumbers(request.Product.ProductId, request.Product.Quantity);
				if (palletAmountFullResult.Success is false) return IssueResult.Fail(palletAmountFullResult.Message);
				var amountPallets = palletAmountFullResult.FullPallet;
				var rest = palletAmountFullResult.Rest;

				//3. Pobierz dostępne palety
				var availablePalletsQuery = await _getAvailable.GetPallets(request.Product.ProductId, request.Product.BestBefore, amountPallets);
				//var availablePalletsQuery = await _mediator.Send(new GetAvailablePalletsByProductQuery(request.Product.ProductId, request.Product.BestBefore, amountPallets, request.Product.Quantity), ct);
				
				//3.1 pobierz dostępne virtualPallet
				var availableVirtualPalletsQuery = await _getVirtualPalletsService.GetVirtualPalletsAsync(request.Product.ProductId, request.Product.BestBefore);

				if (availableVirtualPalletsQuery is null) return IssueResult.Fail("Brak palety do pickingu - błąd virtual");
				//4. Przydziel pełne palety
				//var palletAssigned = await _mediator.Send(new AssignFullPalletToIssueCommand(request.Issue, availablePalletsQuery, amountPallets), ct);
				await _assignFullPalletToIssueService.AddPallets( request.Issue, availablePalletsQuery);
				//5. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
				if (rest > 0)
				{
					var newPickingTaskFromRest =
						await _addPickingTaskToIssueService.AddPickingTaskToIssue(availablePalletsQuery,
						availableVirtualPalletsQuery, request.Issue,
						request.Product.ProductId,
						rest, request.Product.BestBefore,
						request.Issue.PerformedBy
						);
					if (newPickingTaskFromRest.Success is false)
					{
						return IssueResult.Fail(newPickingTaskFromRest.Message);
					}
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, CancellationToken.None);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(factory(), ct);
				}
				await transaction.CommitAsync(ct);
				return IssueResult.Ok("Towar dołączono do wydania", request.Product.ProductId);
			}
			catch (NotFoundProductException expr)//
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(
					expr.Message,
					request.Product.ProductId,
					request.Product.Quantity,
					totalAvailable);
			}
			catch (NotFoundPalletException expal)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(
					expal.Message,
					request.Product.ProductId);
			}
			catch (NotFoundIssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(
					ei.Message, request.Product.ProductId);
			}
			catch (DbUpdateConcurrencyException)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail("Inny użytkownik operuje ...");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas przypisywania palet do zlecenia.", ex.InnerException);
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
