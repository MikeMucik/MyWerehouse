using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Commands.ProcessPickingAction;
using MyWerehouse.Application.PickingPallets.Commands.ReduceAllocation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteManualPicking
{
	public class ExecuteManualPickingHandler : IRequestHandler<ExecuteManualPickingCommand, PickingResult>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		public ExecuteManualPickingHandler(IPalletRepo palletRepo,
			IAllocationRepo allocationRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator,
			IEventCollector eventCollector)
		{
			_palletRepo = palletRepo;
			_allocationRepo = allocationRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
			_eventCollector = eventCollector;
		}
		public async Task<PickingResult> Handle(ExecuteManualPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId)
					?? throw new PalletException(request.PalletId);
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
					?? throw new IssueException(request.IssueId);
				var product = pallet.ProductsOnPallet.FirstOrDefault()
					?? throw new InvalidOperationException($"Paleta {request.PalletId} jest pusta.");

				// Oblicz, ile faktycznie można/trzeba skompletować
				var allocationsForIssue = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(request.IssueId, product.ProductId);
				var neededQuantity = allocationsForIssue.Where(a => a.PickingStatus == PickingStatus.Allocated).Sum(a => a.Quantity);
				var quantityToPick = Math.Min(neededQuantity, product.Quantity);

				if (quantityToPick <= 0)
				{
					return PickingResult.Fail("Brak zapotrzebowania na ten produkt dla wybranego zlecenia.");
				}

				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletId));

				if (virtualPallet == null || virtualPallet.Id == 0)
				{
					virtualPallet = new VirtualPallet
					{
						Pallet = pallet,
						PalletId = pallet.Id,
						DateMoved = DateTime.UtcNow,
						LocationId = pallet.LocationId,
						IssueInitialQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
						Allocations = new List<Allocation>()
						// Dodaj inne wymagane pola (np. Status, CreatedAt = DateTime.UtcNow)
					};

					_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
				}
				//await ReduceAllocationAsync(issue, product.ProductId, quantityToPick, userId);
				await _mediator.Send(new ReduceAllocationCommand(issue, product.ProductId, quantityToPick, request.UserId), ct);
				//await ProcessPickingActionAsync(pallet, issue, product.ProductId, quantityToPick, userId);
				await _mediator.Send(new ProcessPickingActionCommand(pallet, issue, product.ProductId, quantityToPick, request.UserId), ct);
				// Ta logika jest specyficzna dla manuala (tworzenie nowej alokacji)
				var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToPick);
				_allocationRepo.AddAllocation(newAllocation);

				newAllocation.PickingStatus = PickingStatus.Picked;
				var historyPicking = new HistoryDataPicking
							(
								newAllocation.Id,
								newAllocation.VirtualPallet.PalletId,
								newAllocation.IssueId,
									 newAllocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 newAllocation.Quantity,
									 0,
									 PickingStatus.Available,
									 newAllocation.PickingStatus,
								request.UserId,
									 DateTime.UtcNow
								);
				_eventCollector.AddDeferred(async () =>
						new CreateHistoryPickingNotification(
							new HistoryDataPicking
							(
								newAllocation.Id,
								newAllocation.VirtualPallet.PalletId,
								newAllocation.IssueId,
									 newAllocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 newAllocation.Quantity,
									 0,
									 PickingStatus.Available,
									 newAllocation.PickingStatus,
									request.UserId,
									 DateTime.UtcNow
								)));

				await _werehouseDbContext.SaveChangesAsync(ct);
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, ct);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(await factory(), ct);
				}
				//_eventCollector.Clear();
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);

				return PickingResult.Ok("Towar dołączono do zlecenia");
			}
			catch (PalletException pnfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(pnfEx.Message);
			}
			catch (IssueException onfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(onfEx.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
