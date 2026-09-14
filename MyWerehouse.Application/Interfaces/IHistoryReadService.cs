using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Histories.DTOs;

namespace MyWerehouse.Application.Interfaces
{
	public interface IHistoryReadService
	{
		Task<PalletHistoryDTO?> GetHistoryPallet(string palletNumber, int pageNumber, int pageSize, CancellationToken ct);
	}
}
