using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Picking.Queries.PrepareEmergencyPicking
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
				return AppResult<PrepareCorrectedPickingResult>.Fail($"Brak palety na stanie magazynu.", ErrorType.NotFound);
			}
			if (pallet.Status == PalletStatus.Archived || pallet.Status == PalletStatus.OnHold)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta jest zablokowana, brak możliwości operacji.", ErrorType.Conflict);
			}
			var checkPallet = pallet.ProductsOnPallet.Count;
			if (checkPallet > 1)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta nie jest do pickingu, zawiera różne towary", ErrorType.Validation);
			}
					
			var product = pallet.ProductsOnPallet.FirstOrDefault();
			if (product == null)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail("Paleta jest pusta.", ErrorType.NotFound);
			}
			// Logika wyszukiwania pasujących zleceń				
			var timeFrom = request.Start;
			var timeTo = request.End;			
			var pickingTasks  = await _pickingTaskRepo.GetPickingTasksProductIdAsync(product.ProductId, timeFrom, timeTo);
			var grouped = pickingTasks
				.Where(i =>	i.Issue.IssueStatus == IssueStatus.New ||
							i.Issue.IssueStatus == IssueStatus.Pending ||
							i.Issue.IssueStatus == IssueStatus.InProgress)
				.GroupBy(a => new
				{
					a.IssueId,
					a.Issue.IssueNumber
				})
				.Select(g => new IssueOptions
				{
					IssueId = g.Key.IssueId,
					IssueNumber = g.Key.IssueNumber,
					QuantityToDo = g.Sum(a => a.RequestedQuantity - a.PickedQuantity)
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