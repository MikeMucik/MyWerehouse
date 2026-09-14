using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Server.ServicesToInfrastructure
{
	public class HistoryReadService(WerehouseDbContext werehouseDbContext) : IHistoryReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<PalletHistoryDTO?> GetHistoryPallet(string palletNumber, int pageNumber, int pageSize, CancellationToken ct)
		{

			var query = _werehouseDbContext.HistoryPallet
				.AsNoTracking()
				.Where(p => p.PalletNumber == palletNumber)
				.Select(h => new HistoryPalletDTO
				{
					PalletNumber = h.PalletNumber,
					Id = h.Id,
					LocationSnapShotSource = h.SourceLocationSnapShot,
					LocationSnapShotDestination = h.DestinationLocationSnapShot,
					MovementDate = h.MovementDate,
					PerformedBy = h.PerformedBy,
					Reason = h.Reason,
					HistoryPalletDetailsDTO = h.HistoryPalletDetails
					.Select(hd => new HistoryPalletDetailDTO
					{
						ProductId = hd.ProductId,
						QuantityChange = hd.QuantityChange
					}).ToList()
				}).OrderBy(h => h.MovementDate)
				.ThenBy(h=>h.Id);
			var pagedQuery = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

			var result = _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(p => p.PalletNumber == palletNumber)
				.Select(hg => new PalletHistoryDTO
				{
					Id = hg.Id,
					PalletNumber = hg.PalletNumber,
					DateReceived = hg.DateReceived,
					IssueId = hg.IssueId,
					IssueNumber = hg.Issue != null ? (int)hg.Issue.IssueNumber : null,
					ReceiptId = hg.ReceiptId,
					ReceiptNumber = hg.Receipt != null ? (int)hg.Receipt.ReceiptNumber : null,
					PalletMovementsDTO = pagedQuery
				})
				.FirstOrDefaultAsync(ct);
			return await result;
		}
	}
}
