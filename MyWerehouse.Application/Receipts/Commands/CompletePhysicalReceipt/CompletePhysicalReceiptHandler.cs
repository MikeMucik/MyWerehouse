using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt
{
	public class CompletePhysicalReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo) : IRequestHandler<CompletePhysicalReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(CompletePhysicalReceiptCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
				if (receipt == null)
					return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);

				receipt.CompletePhysicalReceipt(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				return AppResult<Unit>.Success(Unit.Value, "Zakończono fizyczne przyjęcie - gotowe do weryfikacji");
			}
			catch (DomainException exd)
			{
			 return	AppResult<Unit>.Fail(exd.Message, ErrorType.Technical);

			}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				return AppResult<Unit>.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				//throw;
			}
		}
	}
}
