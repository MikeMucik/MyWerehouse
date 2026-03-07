using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public class GetReceiptsHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptsQuery, AppResult< List<ReceiptDTO>>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<List<ReceiptDTO>>> Handle(GetReceiptsQuery request, CancellationToken ct)
		{
			var receipts = await _receiptRepo.GetReceiptByFilter(request.Filter).ToListAsync(ct);
			if (receipts.Count == 0) return AppResult<List<ReceiptDTO>>.Fail($"Brak przyjęć do wyświetlenia.", ErrorType.NotFound);

			var dto = _mapper.Map<List<ReceiptDTO>>(receipts);
			return AppResult<List<ReceiptDTO>>.Success(dto);
		}
	}
}
