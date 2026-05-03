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
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public class GetReceiptsHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptsQuery, AppResult<PagedResult<ReceiptDTO>>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<PagedResult<ReceiptDTO>>> Handle(GetReceiptsQuery request, CancellationToken ct)
		{
			var receipts = _receiptRepo.GetReceiptByFilter(request.Filter);
			var receiptsOrdered = receipts.OrderBy(r => r.Id);

			var result = await receiptsOrdered.ToPagedResultAsync<Receipt, ReceiptDTO>(
				_mapper.ConfigurationProvider,
				request.CurrentPage,
				request.PageSize,
				ct);

			if (result.TotalCount == 0) return AppResult<PagedResult<ReceiptDTO>>.Fail($"Brak przyjęć do wyświetlenia.", ErrorType.NotFound);

			return AppResult<PagedResult<ReceiptDTO>>.Success(result);
		}
	}
}
