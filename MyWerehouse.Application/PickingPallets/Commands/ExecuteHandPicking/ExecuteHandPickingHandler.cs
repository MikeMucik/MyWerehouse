using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking
{
	public class ExecuteHandPickingHandler : IRequestHandler<ExecuteHandPickingCommand, PickingResult>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		//private readonly IHandPickingTaskRepo _handPickingTaskRepo;

		public ExecuteHandPickingHandler(IPalletRepo palletRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator,
			IAddPickingTaskToIssueService addPickingTaskToIssueService,
			IProcessPickingActionService processPickingActionService,
			IPickingTaskRepo pickingTaskRepo
			//IHandPickingTaskRepo handPickingTaskRepo
			)
		{
			_palletRepo = palletRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
			_processPickingActionService = processPickingActionService;
			_pickingTaskRepo = pickingTaskRepo;
			//_handPickingTaskRepo = handPickingTaskRepo;
		}
		public async Task<PickingResult> Handle(ExecuteHandPickingCommand command, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletIdSource)
					?? throw new NotFoundPalletException(command.PalletIdSource);
				
				if (pallet.ProductsOnPallet.Count > 1)
				{
					return PickingResult.Fail("Zadania nie można zrealizować, paleta nie nadaje się do pobrań.");
				}
				var issue = await _issueRepo.GetIssueByIdAsync(command.IssueId)
					?? throw new NotFoundIssueException(command.IssueId);
				var product = pallet.ProductsOnPallet.FirstOrDefault()//
					?? throw new InvalidOperationException($"Paleta {command.PalletIdSource} jest pusta.");
				
				if (command.Quanitity > pallet.ProductsOnPallet.First().Quantity)//
				{
					return PickingResult.Fail("Zadania nie można zrealizować, mniej na palecie niż chęć pobrania");
				}
				
				//var pickingHand = await _handPickingTaskRepo.GetByIssueAndProductAsync(command.IssueId, product.Id);
				var pickingHand = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(command.IssueId, product.Id);
				var pickingTask = pickingHand
					.Where(a=>a.VirtualPallet == null)
					.First();
				if (pickingHand == null)
				{
					return PickingResult.Fail("Brak zapotrzebowania na ten asortyment."); 
				}
				if (command.Quanitity > (pickingTask.RequestedQuantity - pickingTask.PickedQuantity))
				{
					return PickingResult.Fail("Chcesz pobrać więcej niż potrzeba.");
				}
				if (pickingTask.PickingStatus == PickingStatus.Picked)
				{
					return PickingResult.Fail("Zapotrzebowania na ten asortyment już zrealizowane");
				}
				VirtualPallet virtualPallet;
				var vpId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(command.PalletIdSource);
				if (vpId != 0)
				{
					int virtualPalletId = vpId;
					
					virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(virtualPalletId);					
				}
				else
				{
					virtualPallet = new VirtualPallet
					{
						//Pallet = pallet,
						PalletId = pallet.Id,
						DateMoved = DateTime.UtcNow,
						LocationId = pallet.LocationId,
						InitialPalletQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
						PickingTasks = [pickingTask]
					};
					_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
				}
				if (command.Quanitity > virtualPallet.RemainingQuantity)
				{
					return PickingResult.Fail("Zadania nie można zrealizować, mniej na palecie niż chęć pobrania");
				}				
				pickingTask.VirtualPallet = virtualPallet;
				var completion = PickingCompletion.Full;
				if (command.Quanitity < pickingTask.RequestedQuantity - pickingTask.PickedQuantity)
				{
					completion = PickingCompletion.Partial;
				}
			 	await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, command.Quanitity, command.UserId, pickingTask, completion);
								
				pickingTask.AddHistory(command.UserId, PickingStatus.Allocated, pickingTask.PickingStatus, command.Quanitity);
			
				await _werehouseDbContext.SaveChangesAsync(ct);
				
				await transaction.CommitAsync(ct);

				return PickingResult.Ok("Towar dołączono do zlecenia");

			}
			catch (NotFoundPalletException pnfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(pnfEx.Message);
			}
			catch (NotFoundIssueException onfEx)
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
		}
	}

}
