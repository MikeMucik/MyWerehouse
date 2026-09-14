using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking;
using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Infrastructure.Persistence.ReadServices
{
	public class ReversePickingReadService(WerehouseDbContext werehouseDbContext) : IReversePickingReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		public async Task<List<ReversePickingPalletWithLocationDTO>> GetListPalletsToReversePicking(DateOnly start, DateOnly end, CancellationToken ct)
		{

			var list = new List<ReversePickingPalletWithLocationDTO>();

			var palletIds = _werehouseDbContext.ReversePickings
				.Where(r => r.Status == ReversePickingStatus.Ongoing && r.DateMade >= start && r.DateMade <= end)
				.Select(r => r.PickingPalletId)
				.Distinct();


			var pallets = _werehouseDbContext.Pallets
				.Where(p => palletIds.Contains(p.Id))
				.Select(p => new ReversePickingPalletWithLocationDTO
				{
					PalletId = p.Id,
					PalletNumber = p.PalletNumber,
					LocationId = p.LocationId,
					LocationName = p.Location.ToSnapshot(),
					Status = p.Status,
				});

			var listToShow = await pallets
				.OrderBy(l => l.LocationId)
					.ThenBy(i => i.PalletId)
				.ToListAsync(ct);
			return listToShow;
		}

		public Task<ReversePickingDTO?> GetReversePicking(Guid reversePickingId,
				CancellationToken ct)
		{
			var result = _werehouseDbContext.ReversePickings
				.AsNoTracking()
				.Where(r => r.Id == reversePickingId)
				.Select(r => new ReversePickingDTO
				{
					Id = r.Id,
					PickingPalletNumber = r.PickingTask.PickingPallet!.PalletNumber,
					SourcePalletNumber = r.PickingTask.VirtualPallet!.Pallet.PalletNumber,
					ProductSKU = r.PickingTask.Product.SKU,
					BestBefore = r.BestBefore,
					Quantity = r.Quantity,
					Status = r.Status,
				})
				.FirstOrDefaultAsync(ct);
			return result;
		}

		public Task<PagedResult<ReversePickingDTO>> GetReversePickingsInDates(DateOnly start, DateOnly end, int pageNumber, int pageSize, CancellationToken ct)
		{
			var result = _werehouseDbContext.ReversePickings
				.AsNoTracking()
				.Where(r => r.Status == ReversePickingStatus.Ongoing && r.DateMade >= start && r.DateMade <= end)
				.OrderBy(r => r.Id)
				.Select(r => new ReversePickingDTO
				{
					Id = r.Id,
					PickingPalletNumber = r.PickingTask.PickingPallet!.PalletNumber,
					SourcePalletNumber = r.PickingTask.VirtualPallet!.Pallet.PalletNumber,
					ProductSKU = r.PickingTask.Product.SKU,
					BestBefore = r.BestBefore,
					Quantity = r.Quantity,
					Status = r.Status,
				});
			return result.ToPagedResultAsync(pageNumber, pageSize, ct);
		}
	}
}
