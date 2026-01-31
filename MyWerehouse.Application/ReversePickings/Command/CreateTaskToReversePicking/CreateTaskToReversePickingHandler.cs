using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Commands;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.ReversePickings.Command.CreateTaskToReversePicking
{
	public class CreateTaskToReversePickingHandler :IRequestHandler<CreateTaskToReversePickingCommand, Unit>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IMediator _mediator;
		public CreateTaskToReversePickingHandler(WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IReversePickingRepo reversePickingRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_reversePickingRepo = reversePickingRepo;
			_mediator = mediator;
		}
		public async Task<Unit> Handle (CreateTaskToReversePickingCommand request, CancellationToken ct)
		{
			//if (await _reversePickingRepo.ExistsForPickingPalletAsync(request.PalletId))
			//	throw new InvalidOperationException("Zadania dekompletacji są już utworzone.");

			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			var listTasks = new List<ReversePicking>();
			//var listResult = new List<ReversePickingResult>();
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId)
				?? throw new NotFoundPalletException(request.PalletId);
			var issue = pallet.Issue
				?? throw new NotFoundIssueException("Brak zlecenia wydania.");
			var pickingTasksOfPickingPallet = await _pickingTaskRepo.GetPickingTasksByPickingPalletIdAsync(request.PalletId);
			if (pickingTasksOfPickingPallet.Count == 0) throw new NotFoundAlloactionException("Brak alokacji dla palety. Paleta nie do dekompletacji.");
			foreach (var pickingTaskToReverse in pickingTasksOfPickingPallet)
			{
				listTasks.Add(new ReversePicking
				{
					PickingPalletId = request.PalletId,
					Quantity = pickingTaskToReverse.RequestedQuantity,
					ProductId = pickingTaskToReverse.ProductId,
					BestBefore = pickingTaskToReverse.BestBefore,
					Status = ReversePickingStatus.Pending,
					PickingTaskId = pickingTaskToReverse.Id,
					UserId =request.UserId,
				});
				//listResult.Add(ReversePickingResult.Ok("Utworzono zadanie dekompletadcji", pickingTaskToReverse.ProductId, request.PalletId));
			}
			foreach (var task in listTasks)
			{
				_reversePickingRepo.AddReversePicking(task);
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			foreach (var task in listTasks)
			{
				var itemHistory = new HistoryReversePickingItem(
					task.Id, task.SourcePalletId, task.DestinationPalletId, issue.Id,
					task.ProductId, task.Quantity, null, task.Status);
				await _mediator.Publish(new CreateHistoryReversePickingNotification(itemHistory, request.UserId), ct);
			}
			return Unit.Value;
				// listResult;
		}
	}
}
