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
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public class GetReceiptsHandler : IRequestHandler<GetReceiptsQuery, List<ReceiptDTO>>
	{
		private readonly IMapper _mapper;
		private readonly IReceiptRepo _receiptRepo;
		public GetReceiptsHandler(IMapper mapper, IReceiptRepo receiptRepo)
		{
			_mapper = mapper;
			_receiptRepo = receiptRepo;
		}
		public async Task<List<ReceiptDTO>> Handle(GetReceiptsQuery request, CancellationToken cancellationToken)
		{
			var receipts = await _receiptRepo.GetReceiptByFilter(request.Filter).ToListAsync();
			return _mapper.Map<List<ReceiptDTO>>(receipts);
		}
	}
}
