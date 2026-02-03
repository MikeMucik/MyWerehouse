using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking
{
	public class ExecutiveReversePickingHandle(WerehouseDbContext werehouseDbContext,
		IReversePickingRepo reversePickingRepo,
		IEventCollector eventCollector,
		IMediator mediator,
		IAddProductsToPalletService addProductsToPalletService) : IRequestHandler<ExecutiveReversePickingCommand, ReversePickingResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IEventCollector _eventCollector = eventCollector;
		private readonly IMediator _mediator = mediator;
		private readonly IAddProductsToPalletService _addProductsToPalletService = addProductsToPalletService;

		public async Task<ReversePickingResult> Handle(ExecutiveReversePickingCommand command, CancellationToken ct)
		{
			var result = new ReversePickingResult();
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var reversePicking = await _reversePickingRepo.GetReversePickingAsync(command.TaskReversedId);
				if (reversePicking is null)
				{
					return ReversePickingResult.Fail("Brak zadania do dekompletacji");
				}
				reversePicking.Status = ReversePickingStatus.InProgress;
				string? sourcePalletId = null;
				string? destinationPalletId = null;
				var issueId = reversePicking.PickingTask.IssueId;
				if (issueId == 0)
					throw new NotFoundIssueException(reversePicking.PickingTask.IssueId);
				switch (command.Strategy)
				{
					case ReversePickingStrategy.ReturnToSource:
						result = _addProductsToPalletService.AddProductsToSourcePallet(reversePicking, command.UserId);
						var virtualPalletPickingTasks = reversePicking.PickingTask.VirtualPallet.PickingTasks;
						var palletFromSource = virtualPalletPickingTasks.First().VirtualPallet.Pallet;
						var hasAnyAllocated = virtualPalletPickingTasks.Any(t=>t.PickingStatus == PickingStatus.Allocated);
						if (!hasAnyAllocated)
						{
							palletFromSource.Status = PalletStatus.Available;
						}									
						break;
					case ReversePickingStrategy.AddToExistingPallet:
						if (command.Pallets.Count == 0) throw new NotFoundPalletException("Brak palet do których można dodać towar.");
						result = await _addProductsToPalletService.AddToExistingPallet(reversePicking, command.Pallets, command.UserId);
						//TODO front co potrzebuje						
						break;
					case ReversePickingStrategy.AddToNewPallet:
						result = await _addProductsToPalletService.AddToNewPallet(reversePicking, command.UserId);
						break;

					//default
				}
				reversePicking.Status = ReversePickingStatus.Completed;

				var debugEntries = _werehouseDbContext.ChangeTracker.Entries()
				.Where(e => e.State != EntityState.Unchanged) // Pokaż tylko to, co EF chce zmienić
				.Select(e => new
				{
					Entity = e.Entity.GetType().Name,
					State = e.State,
					DebugView = e.DebugView.ShortView
				})
				.ToList();

				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				var history = new HistoryReversePickingItem(reversePicking.Id,
					reversePicking.SourcePalletId,
					reversePicking.DestinationPalletId,
					issueId,
					reversePicking.Quantity,
					reversePicking.ProductId,
					ReversePickingStatus.InProgress,
					ReversePickingStatus.Completed);
				await _mediator.Publish(new CreateHistoryReversePickingNotification(history, command.UserId), ct);
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, ct);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(factory(),ct);
				}
				return result;
			}
			catch (NotFoundIssueException ie)
			{
				await transaction.RollbackAsync(ct);
				return ReversePickingResult.Fail(ie.Message);
			}
			catch (NotFoundPalletException pe)
			{
				await transaction.RollbackAsync(ct);
				return ReversePickingResult.Fail(pe.Message);
			}
			catch (NotFoundProductException proe)
			{
				await transaction.RollbackAsync(ct);
				return ReversePickingResult.Fail(proe.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas wykonywania dekompletacji.", ex);
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
