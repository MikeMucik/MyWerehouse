using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking
{
	public class ExecuteHandPickingHandler : IRequestHandler<ExecuteHandPickingCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IProcessPickingActionService _processPickingActionService;
		private readonly IPickingTaskRepo _pickingTaskRepo;

		public ExecuteHandPickingHandler(IPalletRepo palletRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IProcessPickingActionService processPickingActionService,
			IPickingTaskRepo pickingTaskRepo
			)
		{
			_palletRepo = palletRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_processPickingActionService = processPickingActionService;
			_pickingTaskRepo = pickingTaskRepo;
		}
		public async Task<AppResult<Unit>> Handle(ExecuteHandPickingCommand command, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletIdSource);
				if(pallet == null )
				{
					return AppResult<Unit>.Fail($"Paleta o numerze {command.PalletIdSource} nie istnieje.", ErrorType.NotFound);
				}				

				if (pallet.ProductsOnPallet.Count > 1)
				{
					return AppResult<Unit>.Fail("Zadania nie można zrealizować, paleta nie nadaje się do pobrań.", ErrorType.Conflict);					
				}
				var issue = await _issueRepo.GetIssueByIdAsync(command.IssueId);
				if( issue == null)
				{
					return AppResult<Unit>.Fail($"Zamówienie o numerze {command.IssueId} nie zostało znalezione.", ErrorType.NotFound);
				}
				var product = pallet.ProductsOnPallet.FirstOrDefault();//
					if( product == null )
				{
					return AppResult<Unit>.Fail($"Paleta {command.PalletIdSource} jest pusta.", ErrorType.Conflict);
				}			
				if (command.Quanitity > pallet.ProductsOnPallet.First().Quantity)//
				{
					return AppResult< Unit>.Fail("Zadania nie można zrealizować, mniej na palecie niż chęć pobrania", ErrorType.Conflict);
				}
				var pickingHand = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(command.IssueId, product.ProductId);
				var pickingTask = pickingHand
					.Where(a => a.VirtualPallet == null)
					.First();
				if (pickingHand == null)
				{
					return AppResult< Unit>.Fail("Brak zapotrzebowania na ten asortyment.", ErrorType.Conflict);
				}
				if (command.Quanitity > (pickingTask.RequestedQuantity - pickingTask.PickedQuantity))
				{
					return AppResult< Unit>.Fail("Chcesz pobrać więcej niż potrzeba.", ErrorType.Conflict);
				}
				if (pickingTask.PickingStatus == PickingStatus.Picked)
				{
					return AppResult< Unit>.Fail("Zapotrzebowania na ten asortyment już zrealizowane", ErrorType.Conflict);
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
					return AppResult< Unit>.Fail("Zadania nie można zrealizować, mniej na palecie niż chęć pobrania", ErrorType.Conflict);
				}
				pickingTask.SetVirtualPallet(virtualPallet.Id);
				//pickingTask.VirtualPallet = virtualPallet;
				var completion = PickingCompletion.Full;
				if (command.Quanitity < pickingTask.RequestedQuantity - pickingTask.PickedQuantity)
				{
					completion = PickingCompletion.Partial;
				}
				await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, command.Quanitity, command.UserId, pickingTask, completion, command.NumberRamp);
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(pickingTask.VirtualPallet.PalletId);
				pickingTask.AddHistory(command.UserId, sourcePallet.Id, sourcePallet.PalletNumber,issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);

				//pickingTask.AddHistory(command.UserId, PickingStatus.Allocated, pickingTask.PickingStatus, command.Quanitity);

				await _werehouseDbContext.SaveChangesAsync(ct);

				await transaction.CommitAsync(ct);

				return AppResult< Unit>.Success(Unit.Value,"Towar dołączono do zlecenia");

			}
			//catch (NotFoundPalletException pnfEx)
			//{
			//	await transaction.RollbackAsync(ct);
			//	return PickingResult.Fail(pnfEx.Message);
			//}
			//catch (NotFoundIssueException onfEx)
			//{
			//	await transaction.RollbackAsync(ct);
			//	return PickingResult.Fail(onfEx.Message);
			//}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return AppResult< Unit>.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.", ErrorType.Technical);
			}
		}
	}

}
