using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Commands;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.ReversePickings.Command.CreateTaskToReversePicking
{
	public class CreateTaskToReversePickingHandler :IRequestHandler<CreateTaskToReversePickingCommand, Unit>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IMediator _mediator;
		public CreateTaskToReversePickingHandler(WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IAllocationRepo allocationRepo,
			IReversePickingRepo reversePickingRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_allocationRepo = allocationRepo;
			_reversePickingRepo = reversePickingRepo;
			_mediator = mediator;
		}
		public async Task<Unit> Handle (CreateTaskToReversePickingCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			var listTasks = new List<ReversePicking>();
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId)
				?? throw new PalletException(request.PalletId);
			var issue = pallet.Issue
				?? throw new IssueException("Brak zlecenia wydania.");
			foreach (var residue in pallet.ProductsOnPallet)
			{
				var allocations = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issue.Id, residue.ProductId);
				foreach (var allocation in allocations)
				//Dla każdej wykonanej alokacji stwórz zadanie odwrotne
				{
					listTasks.Add(new ReversePicking
					{
						PickingPalletId = request.PalletId,
						Quantity = allocation.Quantity,
						ProductId = residue.ProductId,
						BestBefore = residue.BestBefore,
						Status = ReversePickingStatus.Pending,
						AllocationId = allocation.Id,
					});
				}
				foreach (var task in listTasks)
				{
					 _reversePickingRepo.AddReversePicking(task);
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				foreach (var task in listTasks)
				{
					var itemHistory = new HistoryReversePickingItem
					(
						task.Id,
						task.SourcePalletId,
						task.DestinationPalletId,
						issue.Id,
						task.ProductId,
						task.Quantity,
						null,
						task.Status
					);
					await _mediator.Publish(new CreateHistoryReversePickingNotification(itemHistory, request.UserId), ct);
				}
			}
			return Unit.Value;
		}
	}
}
