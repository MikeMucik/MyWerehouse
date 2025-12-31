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
{//Lista ile danego towaru dla danej alokacji Product's list by allocations
	public class GetListToPickingHandler(IPickingPalletRepo pickingPalletRepo,
		IIssueRepo issueRepo) : IRequestHandler<GetListToPickingQuery, List<ProductToIssueDTO>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<List<ProductToIssueDTO>> Handle (GetListToPickingQuery request, CancellationToken ct)
		{
			var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(request.DateIssueStart, request.DateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return new List<ProductToIssueDTO>();
			}
			var allNededIssuesIds = pickingPallets
				.SelectMany(p => p.Allocations)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);

			var issueDictionary = allIssues.ToDictionary(i => i.Id);

			var aggregationDictionary = new Dictionary<(int ClientId, int
				IssueId, int product), ProductToIssueDTO>();

			foreach (var pallet in pickingPallets)
			{
				var productOnPallet = pallet.Pallet?.ProductsOnPallet?.FirstOrDefault();
				if (productOnPallet == null) continue;
				var productId = productOnPallet.ProductId;

				var allocations = pallet.Allocations;
				foreach (var allocation in allocations)
				{
					if (!issueDictionary.TryGetValue(allocation.IssueId, out var issue))
					{
						continue;
					}
					var clientId = issue.ClientId;
					var key = (clientId, allocation.IssueId, productId);
					if (aggregationDictionary.TryGetValue(key, out var existingRecord))
					{
						existingRecord.Quantity += allocation.Quantity;
					}
					else
					{
						var productIssue = new ProductToIssueDTO
						{
							ClientIdOut = clientId,
							IssueId = allocation.IssueId,
							ProductId = productId,
							Quantity = allocation.Quantity,
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
