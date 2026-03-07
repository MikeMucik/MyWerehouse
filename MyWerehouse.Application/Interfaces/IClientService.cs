using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IClientService
	{		
		Task<AppResult<int>> AddClientAsync(AddClientDTO addClient);		
		Task<AppResult<Unit>> DeleteClientAsync(int id);
		Task<AppResult<AddClientDTO>> GetClientToEditAsync(int id);
		Task<AppResult<Unit>> UpdateClientAsync(UpdateClientDTO updatedClient);
		Task<AppResult<DetailsOfClientDTO>> DetailsOfClientAsync(int id);
		Task<AppResult<ListClientsDTO>> GetClientsByFilterAsync(int pageSize, int PageNumber, ClientSearchFilter filter);		
		Task<AppResult<ListClientsDTO>> GetAllClientsAsync(int pageSize, int PageNumber);
	}
}
