using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public class GetReceiptsHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptsQuery, List<ReceiptDTO>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<List<ReceiptDTO>> Handle(GetReceiptsQuery request, CancellationToken ct)
		{
			var receipts = await _receiptRepo.GetReceiptByFilter(request.Filter).ToListAsync(ct);
			return _mapper.Map<List<ReceiptDTO>>(receipts);
		}
	}
}
