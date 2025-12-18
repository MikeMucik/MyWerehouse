using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CancelIssue
{
	public class CancelIssueHandler : IRequestHandler<CancelIssueCommand, IssueResult>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public readonly IMediator _mediator;
		public readonly IReversePickingService _reversePickingService;
		public CancelIssueHandler(IIssueRepo issueRepo,
			IAllocationRepo allocationRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator,
			IReversePickingService reversePickingService)
		{
			_issueRepo = issueRepo;
			_allocationRepo = allocationRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
			_reversePickingService = reversePickingService;
		}
		public async Task<IssueResult> Handle(CancelIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new IssueException(request.IssueId);
				var listPallet = new List<Pallet>();
				//anulowanie zlecenia dla pełnych palet
				foreach (var pallet in issue.Pallets)
				{
					if (pallet.ReceiptId != null)//paleta kompletacyjna nie ma ReceiptId tylko pełne palety z przyjęcia
					{
						pallet.Status = PalletStatus.Available;
						pallet.IssueId = null;
						pallet.LocationId = 1;//lokalizacja rampy
						listPallet.Add(pallet);
					}
				}
				//palety kompletacyjne i zadania pickingu
				var restPallets = issue.Pallets.Except(listPallet).ToList();

				foreach (var p in restPallets)
				{
					if (p.Status == PalletStatus.Picking)
					{
						//zadanie do reversePicking - tu bedzie handler
						await _reversePickingService.CreateTaskToReversePickingAsync(p.Id, request.UserId);
					}
				}
				//usuń alokacje jeśli nie zrobione
				var allocations = await _allocationRepo.GetAllocationsByIssueIdAsync(request.IssueId);
				foreach (var allocation in allocations)
				{
					_allocationRepo.DeleteAllocation(allocation);
				}
				//usuń virtualPallet jeśli należy tylko do tego zlecenia
				var virtualPallets = await _allocationRepo.GetVirtualPalletsByIssue(request.IssueId);
				foreach (var vp in virtualPallets)
				{
					if (vp.Allocations.Count == 0)
					{
						_pickingPalletRepo.DeleteVirtualPalletPicking(vp);
					}
				}
				issue.IssueStatus = IssueStatus.Cancelled;
				issue.PerformedBy = request.UserId;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId),ct);
				foreach (var p in listPallet)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(p.Id, 1, ReasonMovement.CancelIssue, request.UserId, PalletStatus.Available, null), ct);
				}
				return IssueResult.Ok($"Anulowano zlecenie {request.IssueId}.");
			}
			catch (IssueException ie)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ie.Message);
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
