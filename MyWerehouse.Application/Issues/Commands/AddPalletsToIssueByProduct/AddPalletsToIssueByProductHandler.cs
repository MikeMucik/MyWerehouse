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
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.Queries.GetProductCount;
using MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue;
using MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct;
using MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue;
using MyWerehouse.Application.PickingPallets.Qeuries.GetVirtualPallets;
using MyWerehouse.Application.Products.Queries.GetNumberUnitOnPallet;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.AddPalletsToIssueByProduct
{
	public class AddPalletsToIssueByProductHandler : IRequestHandler<AddPalletsToIssueByProductCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		public AddPalletsToIssueByProductHandler(WerehouseDbContext werehouseDbContext,
			IMediator mediator,
			IEventCollector eventCollector)
		{
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
			_eventCollector = eventCollector;			
		}
		public async Task<IssueResult> Handle(AddPalletsToIssueByProductCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
			var totalAvailable = 0;
			try
			{
				if (request.Issue.IssueStatus == IssueStatus.New)request.Issue.IssueStatus = IssueStatus.Pending;
				if (request.Issue.IssueStatus != IssueStatus.Pending && request.Issue.IssueStatus != IssueStatus.New)
				{
					throw new IssueException("Błąd statusu zlecenia");
				}

				//1 dostepność towaru - ile jest opakowań z datą dłuższą niż BestBefore
				totalAvailable = await _mediator.Send(new GetProductCountQuery(request.Product.ProductId, request.Product.BestBefore), ct);
				if (request.Product.Quantity > totalAvailable)
				{
					throw new ProductException($"Nie wystarczająca ilości produktu o numerze {request.Product.ProductId}. Asortyment nie został dodany do zlecenia.");
				}
				//2 Oblicz pełne palety i resztę - to można wyodrębnić 
				var palletAmountFullResult = _mediator.Send(new GetNumberPalletsAndRestQuery(request.Product.ProductId, request.Product.Quantity), ct);
								
				var amountPallets = palletAmountFullResult.Result.FullPallet;
				var rest = palletAmountFullResult.Result.Rest;

				//3. Pobierz dostępne palety
				var availablePalletsQuery = await _mediator.Send(new GetAvailablePalletsByProductQuery(request.Product.ProductId, request.Product.BestBefore, amountPallets, request.Product.Quantity), ct);
				//3.1 pobierz dostępne virtualPallet
				var availableVirtualPalletsQuery = await _mediator.Send(new GetVirtualPalletsQuery(request.Product.ProductId, request.Product.BestBefore), ct);
				//4. Przydziel pełne palety
				var palletAssigned = await _mediator.Send(new AssignFullPalletToIssueCommand(request.Issue, availablePalletsQuery, amountPallets), ct);
				List<Pallet> restPallet = availablePalletsQuery.Except(palletAssigned).ToList();
				//5. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
				if (rest > 0)
				{
					var newAllocationFromRest = await _mediator.Send(
						new AddAllocationToIssueCommand(restPallet, availableVirtualPalletsQuery, request.Issue,
						request.Product.ProductId,
						rest, request.Product.BestBefore,
						request.Issue.PerformedBy
						), ct);// palety do pickingu
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, CancellationToken.None);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(await factory(), ct);
				}
				await transaction.CommitAsync(ct);

				_eventCollector.Clear();

				return IssueResult.Ok("Towar dołączono do wydania", request.Product.ProductId);
			}
			catch (ProductException expr)//
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(
					expr.Message,
					request.Product.ProductId,
					request.Product.Quantity,
					totalAvailable);
			}
			catch (PalletException expal)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(
					expal.Message,
					request.Product.ProductId);
			}
			catch (IssueException ei)
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
		}
	}
}
