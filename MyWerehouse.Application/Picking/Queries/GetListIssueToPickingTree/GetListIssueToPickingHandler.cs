using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	//Lista produkt ilość, na razie kilka raportów to bez dodatkowego repo etc.
	//Lista ile danego towaru dla danego zlecenia drzewo
	public class GetListIssueToPickingHandler(IPickingTaskRepo pickingTaskRepo, IVirtualPalletRepo virtualPalletRepo) : IRequestHandler<GetListIssueToPickingQuery, AppResult<List<PickingGuideLineDTO>>>
	{
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;

		public async Task<AppResult<List<PickingGuideLineDTO>>> Handle(GetListIssueToPickingQuery request, CancellationToken ct)
		{

			var data = await _pickingTaskRepo.GetPickingTaskFlats(request.DateIssueStart, request.DateIssueEnd)
				.OrderBy(x => x.ClientId)
				.ThenBy(x => x.IssueId)
				.ThenBy(x => x.ProductId)
				.ToListAsync(ct);
			if (data.Count == 0)
			{
				return AppResult<List<PickingGuideLineDTO>>.Fail("Brak elementów do wyświelenia", ErrorType.NotFound);
			}
			var result = data
				.GroupBy(x => x.ClientId)
				.Select(clientGroup => new PickingGuideLineDTO
				{
					ClientIdOut = clientGroup.Key,
					IssuesDetailsForPicking = clientGroup
					.GroupBy(x => x.IssueId)
					.Select(issueGroup => new IssueForPickingDTO
					{
						IssueId = issueGroup.Key,
						IssueNumber = issueGroup.First().IssueNumber,
						Products = issueGroup
						.Select(productItem => new ProductOnPalletPickingDTO
						{
							ProductId = productItem.ProductId,
							Quantity = productItem.Quantity,
						}).ToList(),
					}).ToList(),
				})
				.OrderBy(x=>x.ClientIdOut)
				.ToList();

			return AppResult<List<PickingGuideLineDTO>>.Success(result);
		}
	}
}