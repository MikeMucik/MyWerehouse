using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet
{
	public class ClosePickingPalletHandler(IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<ClosePickingPalletCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<Unit>> Handle(ClosePickingPalletCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
				return AppResult<Unit>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			if (pallet.Status != Domain.Pallets.Models.PalletStatus.Picking)
				return AppResult<Unit>.Fail($"Palety {pallet.Id} nie można zamknąć. Błędny status palet");
			//to powyżej można rozszerzyć na inne przypadki				
			pallet.CloseAndAddPickingPallet(request.IssueId, request.UserId, pallet.Location.ToSnapshot());
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Zamknięto paletę, dołączono do zlecenia {issue.Id}.");
		}
	}
}
