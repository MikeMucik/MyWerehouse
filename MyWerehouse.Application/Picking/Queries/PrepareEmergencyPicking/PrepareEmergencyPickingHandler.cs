using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking
{
	public class PrepareEmergencyPickingHandler(IPalletRepo palletRepo,
		IPickingTaskRepo pickingTaskRepo) : IRequestHandler<PrepareEmergencyPickingQuery, AppResult<PrepareCorrectedPickingResult>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;

		public async Task<AppResult<PrepareCorrectedPickingResult>> Handle(PrepareEmergencyPickingQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			//Nie wyjątek bo to częsta sytuacja w rzeczywistości
			if (pallet == null)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail($"Brak palety na stanie magazynu.");
			}
			if (pallet.Status == PalletStatus.Archived || pallet.Status == PalletStatus.OnHold)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta jest zablokowona, brak możliwości operacji.");
			}
			var checkPallet = pallet.ProductsOnPallet.Count;
			if (checkPallet > 1)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta nie jest do pickingu, zawiera rózne towary");

			}
			if (pallet.Status != PalletStatus.ToPicking) //do uzgodnienia biznesowego
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta nie jest w pickingu, zmień status.");
			}			
			var product = pallet.ProductsOnPallet.FirstOrDefault();
			if (product == null)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta jest pusta.");
			}
			// Logika wyszukiwania pasujących zleceń				
			var timeFrom = request.Start;
			var timeTo = request.End;
			var pickingTasksAll = await _pickingTaskRepo.GetPickingTasksProductIdAsync(product.ProductId, timeFrom, timeTo);
			var pickingTasks = pickingTasksAll.Where(p => p.PickingStatus == PickingStatus.Allocated);
			var grouped = pickingTasks
				.GroupBy(a => new
				{
					a.IssueId,
					a.Issue.IssueNumber
				})
				.Select(g => new IssueOptions
				{
					IssueId = g.Key.IssueId,
					IssueNumber = g.Key.IssueNumber,
					QunatityToDo = g.Sum(a => a.RequestedQuantity)
				})
				.ToList();
			var result = PrepareCorrectedPickingResult.RequiresOrder(
				productInfo: $"{product.PalletId} : {product.Quantity}",
				issueOptions: grouped,
				message: "Podaj numer zamówienia by kontynuować");
			return AppResult<PrepareCorrectedPickingResult>.Success(result);
		}
	}
}