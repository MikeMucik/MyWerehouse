using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public class CreateReversePickingService : ICreateReversePickingService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IReversePickingRepo _reversePickingRepo;	
		public CreateReversePickingService(IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IReversePickingRepo reversePickingRepo)
		{
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_reversePickingRepo = reversePickingRepo;			
		}

		public async Task CreateReversePicking(string palletId, string userId)
		{
			if (await _reversePickingRepo.ExistsForPickingPalletAsync(palletId))
				throw new NotFoundReversePickingException("Zadania dekompletacji są już utworzone.");
			var listTasks = new List<ReversePicking>();
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId)
				?? throw new NotFoundPalletException(palletId);
			var issue = pallet.Issue
				?? throw new NotFoundIssueException("Brak zlecenia wydania.");
			var pickingTasksOfPickingPallet = await _pickingTaskRepo.GetPickingTasksByPickingPalletIdAsync(palletId);
			if (pickingTasksOfPickingPallet.Count == 0) throw new NotFoundPickingTaskException("Brak alokacji dla palety. Paleta nie do dekompletacji.");//
			foreach (var pickingTaskToReverse in pickingTasksOfPickingPallet)
			{
				listTasks.Add(new ReversePicking
				{
					PickingPalletId = palletId,
					Quantity = pickingTaskToReverse.RequestedQuantity,
					ProductId = pickingTaskToReverse.ProductId,
					BestBefore = pickingTaskToReverse.BestBefore,
					Status = ReversePickingStatus.Pending,
					PickingTaskId = pickingTaskToReverse.Id,
					SourcePalletId = pickingTaskToReverse.VirtualPallet.PalletId,
					UserId = userId,
					DateMade = DateOnly.FromDateTime(DateTime.UtcNow)
				});
			}
			foreach (var task in listTasks)
			{
				_reversePickingRepo.AddReversePicking(task);
			}			
			foreach (var task in listTasks)
			{
				task.AddHistory(userId, issue.Id, issue.IssueNumber, ReversePickingStatus.Pending, ReversePickingStatus.Pending);
			}
		}
	}
}
