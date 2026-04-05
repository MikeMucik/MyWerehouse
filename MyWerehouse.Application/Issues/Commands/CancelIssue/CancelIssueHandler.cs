using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.CancelIssue
{
	public class CancelIssueHandler : IRequestHandler<CancelIssueCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly ICreateReversePickingService _createReversePickingService;
		public CancelIssueHandler(IIssueRepo issueRepo,
			IPickingTaskRepo pickingTaskRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			ICreateReversePickingService createReversePickingService
			)
		{
			_issueRepo = issueRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_createReversePickingService = createReversePickingService;
		}
		public async Task<AppResult<Unit>> Handle(CancelIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if (issue == null)
					return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
				var listPallet = new List<Pallet>();
				//anulowanie zlecenia dla pełnych palet
				var listPalletsOfIssue = issue.Pallets.ToList();
				foreach (var pallet in listPalletsOfIssue)
				{
					if (pallet.ReceiptId != null)//paleta kompletacyjna nie ma ReceiptId tylko  palety z przyjęcia
					{
						issue.DetachPallet(pallet, request.UserId);
						listPallet.Add(pallet);
					}
				}
				//palety kompletacyjne i zadania pickingu 
				var restPallets = issue.Pallets.Except(listPallet).ToList();
				foreach (var p in restPallets)
				{
					var resultReverse = await _createReversePickingService.CreateReversePicking(p.Id, request.UserId);
					if (!resultReverse.Success && resultReverse.Message.Contains("Zadania")) return AppResult<Unit>.Fail(resultReverse.Message, ErrorType.Conflict);
					if (!resultReverse.Success ) return AppResult<Unit>.Fail(resultReverse.Message, ErrorType.NotFound);					
				}
				//usuń alokacje/pickingTask jeśli nie zrobione				
				var virtualPallets = await _pickingTaskRepo.GetVirtualPalletsByIssue(request.IssueId);
				foreach (var vp in virtualPallets)
				{
					var pickingTaskToRemove = vp.PickingTasks
						.Where(a => a.PickingStatus == PickingStatus.Allocated && a.IssueId == issue.Id)
						.ToList();
					foreach (var pickingTask in pickingTaskToRemove)
					{
						pickingTask.Cancel(request.UserId, issue.IssueNumber);
						//pickingTask.PickingStatus = PickingStatus.Cancelled;
						//pickingTask.AddHistory(request.UserId, PickingStatus.Allocated, pickingTask.PickingStatus, 0);

						vp.PickingTasks.Remove(pickingTask);
						_pickingTaskRepo.DeletePickingTask(pickingTask);
					}
					//usuń virtualPallet jeśli należy tylko do tego zlecenia
					if (vp.PickingTasks.Count == 0)
					{
						_pickingPalletRepo.DeleteVirtualPalletPicking(vp);
						vp.Pallet.ChangeStatus(PalletStatus.Available);
						//vp.Pallet.Status = PalletStatus.Available;
					}
				}
				issue.Cancel(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, $"Anulowano zlecenie {request.IssueId}.");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	

				throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			}
		}
	}
}
