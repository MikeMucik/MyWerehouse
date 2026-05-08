using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Picking.Queries.GetListToPickingFlat
{//Lista ile danego towaru dla danej alokacji Product's list by pickingTasks
 //klient -> zamówienie -> produkt -> ilośc -  płasko
	public class GetListToPickingHandler(IPickingTaskRepo pickingTaskRepo) : IRequestHandler<GetListToPickingQuery, AppResult<List<ProductToIssueDTO>>>
	{
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;	

		public async Task<AppResult<List<ProductToIssueDTO>>> Handle(GetListToPickingQuery request, CancellationToken ct)
		{
			var data = await _pickingTaskRepo.GetPickingTaskFlats(request.DateIssueStart, request.DateIssueEnd)
				.OrderBy(x => x.ClientId)
				.ThenBy(x => x.IssueId)
				.ThenBy(x => x.ProductId)
				.ToListAsync(ct);

			if (data.Count == 0)
			{
				return AppResult<List<ProductToIssueDTO>>.Fail("Brak elementów do wyświelenia", ErrorType.NotFound);
			}
			var result = data.Select(x => new ProductToIssueDTO
			{
				ClientIdOut = x.ClientId,
				IssueId = x.IssueId,
				IssueNumber = x.IssueNumber,
				ProductId = x.ProductId,
				Quantity = x.Quantity,
			}).ToList();

			return AppResult<List<ProductToIssueDTO>>.Success(result);
		}
	}
}
