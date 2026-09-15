using MediatR;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Picking.Commands.ClosePickingPallet
{
	public class ClosePickingPalletHandler(IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		IUnitOfWork unitOfWork) : IRequestHandler<ClosePickingPalletCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;

		public async Task<AppResult<Unit>> Handle(ClosePickingPalletCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId, ct);
			if (pallet == null)
				return AppResult<Unit>.Fail("The specified pallet does not exist.");
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId, ct);
			if (issue == null)
				return AppResult<Unit>.Fail("The issue for this pallet was not found.");
			pallet.CloseAndAddPickingPallet(request.IssueId, request.UserId, pallet.Location.ToSnapshot());
	
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Pallet was closed and added to issue {issue.IssueNumber}.");
		}
	}
}
