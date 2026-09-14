using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IClientReadService
	{
		Task<PagedResult<ClientDTO>> GetClientsByFilterAsync(
			int pageNumber,
			int pageSize,
			ClientSearchFilter filter,
			CancellationToken ct);
		Task<PagedResult<ClientDTO>> GetAllClientsAsync(
			int pageNumber,
			int pageSize,
			CancellationToken ct);
		Task<DetailsOfClientDTO?> DetailsOdClientAsync(
			int id,
			CancellationToken ct);
		Task<ClientDTO?> GetClientByIdAsync(
			int id,
			CancellationToken ct);
	}
}
