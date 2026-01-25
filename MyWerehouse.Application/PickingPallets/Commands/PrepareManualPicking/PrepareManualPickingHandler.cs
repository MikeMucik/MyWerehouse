using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.PickingPallets.Commands.PrepareManualPicking
{
	public class PrepareManualPickingHandler(IPalletRepo palletRepo,
		IPickingTaskRepo pickingTaskRepo) : IRequestHandler<PrepareManualPickingCommand, PickingResult>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;

		public async Task<PickingResult> Handle(PrepareManualPickingCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			//Nie wyjątek bo to częsta sytuacja w rzeczywistości
			if (pallet == null || pallet.Status == PalletStatus.Archived)
			{
				return PickingResult.Fail($"Brak palety {request.PalletId} na stanie.");
			}

			if (pallet.Status != PalletStatus.ToPicking)
			{
				return PickingResult.Fail($"Paleta {request.PalletId} nie jest w pickingu, zmień status.");
			}

			var product = pallet.ProductsOnPallet.FirstOrDefault();
			if (product == null)
			{
				return PickingResult.Fail($"Paleta {request.PalletId} jest pusta.");
			}
			// Logika wyszukiwania pasujących zleceń			
			var timeFrom = DateTime.UtcNow.AddDays(-1);
			var timeTo = DateTime.UtcNow;
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksProductIdAsync(product.ProductId, timeFrom, timeTo);
			var grouped = pickingTasks
				.GroupBy(a => a.IssueId)
				.Select(g => new IssueOptions
				{
					IssueId = g.Key,
					QunatityToDo = g.Sum(a => a.Quantity)
				})
				.ToList();
			return PickingResult.RequiresOrder(
				productInfo: $"{product.PalletId} : {product.Quantity}",
				issueOptions: grouped,
				message: "Podaj numer zamówienia by kontynuować");
		}
	}
}
