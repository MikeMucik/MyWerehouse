using MyWerehouse.Application.Histories.DTOs;

namespace MyWerehouse.Application.Histories.Services
{
	public interface IHistoryReadService
	{
		Task<PalletHistoryDTO?> GetHistoryPallet(string palletNumber, int pageNumber, int pageSize, CancellationToken ct);
	}
}
