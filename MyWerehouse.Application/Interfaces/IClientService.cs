using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IClientService
	{		
		Task<AppResult<int>> AddClientAsync(ClientDTO addClient);		
		Task<AppResult<Unit>> DeleteClientAsync(int id);
		Task<AppResult<ClientDTO>> GetClientToEditAsync(int id);
		Task<AppResult<Unit>> UpdateClientAsync(UpdateClientDTO updatedClient);
		Task<AppResult<DetailsOfClientDTO>> DetailsOfClientAsync(int id);
		Task<AppResult<PagedResult<ClientDTO>>> GetClientsByFilterAsync(int pageSize, int PageNumber, ClientSearchFilter filter, CancellationToken ct);		
		Task<AppResult<PagedResult<ClientDTO>>> GetAllClientsAsync(int pageSize, int PageNumber, CancellationToken ct);
	}
}
