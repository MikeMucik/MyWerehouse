using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipt
{
	public class GetReceiptByIdHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptByIdQuery, ReceiptDTO>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<ReceiptDTO> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)??
				throw new ReceiptException(request.ReceiptId);			
			var receiptDTO = _mapper.Map<ReceiptDTO>(receipt);
			return receiptDTO;
		}
	}
}
