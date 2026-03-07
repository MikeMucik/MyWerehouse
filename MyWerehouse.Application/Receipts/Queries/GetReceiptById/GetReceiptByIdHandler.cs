using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipt
{
	public class GetReceiptByIdHandler(IMapper mapper, IReceiptRepo receiptRepo) : IRequestHandler<GetReceiptByIdQuery, AppResult< ReceiptDTO>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<ReceiptDTO>> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<ReceiptDTO>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);

			var receiptDTO = _mapper.Map<ReceiptDTO>(receipt);
			return AppResult<ReceiptDTO>.Success(receiptDTO);
		}
	}
}
