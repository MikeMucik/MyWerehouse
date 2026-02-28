using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListToPicking
{//Lista ile danego towaru dla danej alokacji Product's list by pickingTasks
	public class GetListToPickingHandler(IPickingPalletRepo pickingPalletRepo,
		IIssueRepo issueRepo) : IRequestHandler<GetListToPickingQuery, List<ProductToIssueDTO>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<List<ProductToIssueDTO>> Handle (GetListToPickingQuery request, CancellationToken ct)
		{
			var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimePickingTaskAsync(request.DateIssueStart, request.DateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return new List<ProductToIssueDTO>();
			}
			var allNeededIssuesIds = pickingPallets
				.SelectMany(p => p.PickingTasks)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNeededIssuesIds);

			var issueDictionary = allIssues.ToDictionary(i => i.Id);

			var aggregationDictionary = new Dictionary<(int ClientId, Guid
				IssueId, int product), ProductToIssueDTO>();

			foreach (var pallet in pickingPallets)
			{
				var productOnPallet = pallet.Pallet?.ProductsOnPallet?.FirstOrDefault();
				if (productOnPallet == null) continue;
				var productId = productOnPallet.ProductId;

				var pickingTasks = pallet.PickingTasks;
				foreach (var pickingTask in pickingTasks)
				{
					if (!issueDictionary.TryGetValue(pickingTask.IssueId, out var issue))
					{
						continue;
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
			return aggregationDictionary
					.OrderBy(x => x.Key.ClientId)
						.ThenBy(x => x.Key.IssueId)
							.ThenBy(x => x.Key.product)
					.Select(x => x.Value)
					.ToList();
		}
	}
}
