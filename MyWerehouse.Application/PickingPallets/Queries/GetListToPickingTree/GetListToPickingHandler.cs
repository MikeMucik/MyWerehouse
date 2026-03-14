using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListToPicking
{//Lista ile danego towaru dla danej alokacji Product's list by pickingTasks
 //klient -> zamówienie -> produkt -> ilośc - Dictionary - płasko
	public class GetListToPickingHandler(IPickingPalletRepo pickingPalletRepo,
		IIssueRepo issueRepo) : IRequestHandler<GetListToPickingQuery, AppResult<List<ProductToIssueDTO>>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<List<ProductToIssueDTO>>> Handle(GetListToPickingQuery request, CancellationToken ct)
		{
			var virtualPallets = await _pickingPalletRepo.GetVirtualPalletsByTimePickingTaskAsync(request.DateIssueStart, request.DateIssueEnd);
			if (virtualPallets.Count == 0)
			{
				return AppResult<List<ProductToIssueDTO>>.Fail("Brak elementów do wyświelenia", ErrorType.NotFound);
			}
			var allNeededIssuesIds = virtualPallets
				.SelectMany(p => p.PickingTasks)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNeededIssuesIds);

			var issueDictionary = allIssues.ToDictionary(i => i.Id);

			var aggregationDictionary = new Dictionary<(int ClientId, Guid
				IssueId, int product), ProductToIssueDTO>();

			//
			foreach (var virtualPallet in virtualPallets)
			{
				var productOnPallet = virtualPallet.Pallet?.ProductsOnPallet?.FirstOrDefault();
				if (productOnPallet == null) continue;
				var productId = productOnPallet.ProductId;

				var pickingTasks = virtualPallet.PickingTasks;
				foreach (var pickingTask in pickingTasks)
				{
					if (!issueDictionary.TryGetValue(pickingTask.IssueId, out var issue))
					{
						continue; //omijamy ten pickingTask
					}
					var clientId = issue.ClientId;
					var key = (clientId, pickingTask.IssueId, productId);
					if (aggregationDictionary.TryGetValue(key, out var existingRecord))
					{
						existingRecord.Quantity += pickingTask.RequestedQuantity;
					}
					else
					{
						var productIssue = new ProductToIssueDTO
						{
							ClientIdOut = clientId,
							IssueId = pickingTask.IssueId,
							IssueNumber = pickingTask.IssueNumber,
							ProductId = productId,
							Quantity = pickingTask.RequestedQuantity,
						};
						aggregationDictionary.Add(key, productIssue);
					}
				}
			}
			var result = aggregationDictionary
						.OrderBy(x => x.Key.ClientId)
							.ThenBy(x => x.Key.IssueId)
								.ThenBy(x => x.Key.product)
						.Select(x => x.Value)
						.ToList();
			return AppResult<List<ProductToIssueDTO>>.Success(result);
		}
	}
}
