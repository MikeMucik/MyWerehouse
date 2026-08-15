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

namespace MyWerehouse.Application.Picking.Queries.PrepareEmergencyPicking
{
	public class PrepareEmergencyPickingHandler(IPalletRepo palletRepo,
		IPickingTaskRepo pickingTaskRepo) : IRequestHandler<PrepareEmergencyPickingQuery, AppResult<PrepareCorrectedPickingResult>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;

		public async Task<AppResult<PrepareCorrectedPickingResult>> Handle(PrepareEmergencyPickingQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId, ct);
			if (pallet == null)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail($"Pallet was not found in warehouse stock.");
			}
			var product = pallet.EnsureCanBeUsedForPicking();//to jest walidacja palety źródło
			// Logika wyszukiwania pasujących zleceń
			var timeFrom = request.Start;
			var timeTo = request.End;
			var pickingTasks  = await _pickingTaskRepo.GetPickingTasksProductIdAsync(product.ProductId, timeFrom, timeTo, ct);
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
				message: "Provide the issue number to continue.");
			return AppResult<PrepareCorrectedPickingResult>.Success(result);
		}
	}
}
