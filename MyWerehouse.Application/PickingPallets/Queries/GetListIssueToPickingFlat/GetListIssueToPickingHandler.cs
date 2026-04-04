using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPickingBYCLient
{
	//Lista produkt ilość
	//Lista ile danego towaru dla danego zlecenia posegregowane i zgrupowane po kliencie Product's list by issue&client
	public class GetListIssueToPickingHandler(IPickingPalletRepo pickingPalletRepo,
		IIssueRepo issueRepo) : IRequestHandler<GetListIssueToPickingQuery, AppResult< List<PickingGuideLineDTO>>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<List<PickingGuideLineDTO>>> Handle(GetListIssueToPickingQuery request, CancellationToken ct)
		{
			var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimePickingTaskAsync(request.DateIssueStart, request.DateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return  AppResult< List<PickingGuideLineDTO>>.Fail("Brak elementów do wyświetlenia", ErrorType.NotFound);	
			}
			var allNeededIssuesIds = pickingPallets
				.SelectMany(p => p.PickingTasks)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNeededIssuesIds);
			var issueDictionary = allIssues.ToDictionary(i => i.Id);
			var result = new List<PickingGuideLineDTO>( pickingPallets
				.SelectMany(p => p.PickingTasks.Select(a => new
				{
					IssueNumber = a.Issue.IssueNumber,
					Quantity = a.RequestedQuantity,
					ProductId = a.ProductId,
					ClientIdOut = issueDictionary[a.IssueId].ClientId
				}))
				.GroupBy(x => x.ClientIdOut)
				.Select(clientGroup => new PickingGuideLineDTO
				{
					ClientIdOut = clientGroup.Key,
					IssuesDetailsForPicking = [.. clientGroup
						.GroupBy(a => a.IssueNumber)
						.Select(issueGroup => new IssueForPickingDTO
						{
							IssueNumber = issueGroup.Key,
							Products = [.. issueGroup
								.GroupBy(a => a.ProductId)
								.Select(prodGroup => new ProductOnPalletPickingDTO
								{
									ProductId = prodGroup.Key,
									Quantity = prodGroup.Sum(x => x.Quantity)
								})
								.OrderBy(p => p.ProductId)]
						})
						.OrderBy(i => i.IssueNumber)]
				})
				.OrderBy(c => c.ClientIdOut));
			return AppResult<List<PickingGuideLineDTO>>.Success( result);
		}
		
	}
}
