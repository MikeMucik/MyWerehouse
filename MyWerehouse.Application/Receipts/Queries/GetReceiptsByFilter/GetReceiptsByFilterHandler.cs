using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter
{
	public class GetReceiptsByFilterHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptsByFilterQuery, AppResult<PagedResult<ReceiptSimplyDTO>>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<PagedResult<ReceiptSimplyDTO>>> Handle(GetReceiptsByFilterQuery request, CancellationToken ct)
		{
			var receipts = _receiptRepo.GetReceiptByFilter(request.Filter)
				.AsNoTracking();
			var receiptsOrdered = receipts.OrderBy(r => r.Id);
			var result = await receiptsOrdered
				.ProjectTo<ReceiptSimplyDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(request.CurrentPage,request.PageSize,ct);
			if (result.TotalCount == 0) return AppResult<PagedResult<ReceiptSimplyDTO>>.Fail($"Brak przyjęć do wyświetlenia.", ErrorType.NotFound);
			return AppResult<PagedResult<ReceiptSimplyDTO>>.Success(result);
		}
	}
}
