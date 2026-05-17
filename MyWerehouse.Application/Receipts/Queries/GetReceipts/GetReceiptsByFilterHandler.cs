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
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public class GetReceiptsByFilterHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptsByFilterQuery, AppResult<PagedResult<ReceiptDTO>>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<PagedResult<ReceiptDTO>>> Handle(GetReceiptsByFilterQuery request, CancellationToken ct)
		{
			var receipts = _receiptRepo.GetReceiptByFilter(request.Filter)
				.AsNoTracking();
			var receiptsOrdered = receipts.OrderBy(r => r.Id);
			var result = await receiptsOrdered
				.ProjectTo<ReceiptDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(request.CurrentPage,request.PageSize,ct);
			if (result.TotalCount == 0) return AppResult<PagedResult<ReceiptDTO>>.Fail($"Brak przyjęć do wyświetlenia.", ErrorType.NotFound);
			return AppResult<PagedResult<ReceiptDTO>>.Success(result);
		}
	}
}
