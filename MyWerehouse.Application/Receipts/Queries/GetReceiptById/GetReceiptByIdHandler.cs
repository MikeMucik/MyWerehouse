using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipt
{
	public class GetReceiptByIdHandler : IRequestHandler<GetReceiptByIdQuery, ReceiptDTO>
	{
		private readonly IMapper _mapper;
		private readonly IReceiptRepo _receiptRepo;		
		public GetReceiptByIdHandler(IMapper mapper, IReceiptRepo receiptRepo)
		{
			_mapper = mapper;
			_receiptRepo = receiptRepo;			
		}
		public async Task<ReceiptDTO> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);		
			var receiptDTO = _mapper.Map<ReceiptDTO>(receipt);
			return receiptDTO;
		}
	}
}
