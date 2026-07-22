using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public class CreateReversePickingService : ICreateReversePickingService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IDateTimeProvider _dateTimeProvider;
		public CreateReversePickingService(IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IReversePickingRepo reversePickingRepo,
			IDateTimeProvider dateTimeProvider)
		{
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_reversePickingRepo = reversePickingRepo;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<ReversePickingResult> CreateReversePicking(Guid palletId, string userId)
		{
			var nowDateOnly = _dateTimeProvider.Today;    
			if (await _reversePickingRepo.ExistsForPickingPalletAsync(palletId))
				return ReversePickingResult.Ok();
			var listTasks = new List<ReversePickingTask>();
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			if (pallet == null) return ReversePickingResult.Fail($"Paleta o numerze {palletId} nie istnieje.");
			var issue = pallet.Issue;
			if (issue == null) return ReversePickingResult.Fail("Brak zlecenia wydania.");
			issue.EnsureCanBeCancelled();
			var pickingTasksOfPickingPallet = await _pickingTaskRepo.GetPickingTasksByPickingPalletIdAsync(palletId);
			if (pickingTasksOfPickingPallet.Count == 0)
				return ReversePickingResult.Fail("Brak alokacji dla palety. Paleta nie do dekompletacji.");
			foreach (var pickingTaskToReverse in pickingTasksOfPickingPallet)
			{
				listTasks.Add(
					ReversePickingTask.Create(palletId, pickingTaskToReverse.VirtualPallet!.PalletId, pickingTaskToReverse.ProductId, pickingTaskToReverse.BestBefore,
					pickingTaskToReverse.PickedQuantity, pickingTaskToReverse.Id, userId, nowDateOnly));					
			}
			foreach (var task in listTasks)
			{
				_reversePickingRepo.AddReversePicking(task);
			}
			foreach (var task in listTasks)
			{
				task.AddHistory(userId, issue.Id, issue.IssueNumber, ReversePickingStatus.Ongoing, ReversePickingStatus.Ongoing);
			}
			return ReversePickingResult.Ok();
		}
	}
}
