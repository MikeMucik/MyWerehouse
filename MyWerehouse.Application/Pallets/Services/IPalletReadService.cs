using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Pallets.Queries.GetPallet;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Domain.Pallets.Filters;

namespace MyWerehouse.Application.Pallets.Services
{
	public interface IPalletReadService
	{
		Task<PagedResult<PalletSimplyDTO>> GetPalletsByFilterAsync(PalletSearchFilter filter, int currentPage, int pageSize, CancellationToken ct);
		Task<PalletDTO?> GetPalletByIdFullInfoAsync(Guid id, CancellationToken ct);
		Task<ShowPalletToEditDTO?> ShowPalletToEditAsync(Guid id, CancellationToken ct);
		Task<PalletSimplyDTO?> GetPalletByPalletNumberAsync(string palletNumber, CancellationToken ct);
	}
}
