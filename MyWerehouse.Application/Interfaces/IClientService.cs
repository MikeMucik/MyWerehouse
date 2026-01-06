using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IClientService
	{		
		Task<int> AddClientAsync(AddClientDTO addClient);		
		Task DeleteClientAsync(int id);
		Task<AddClientDTO> GetClientToEditAsync(int id);
		Task UpdateClientAsync(UpdateClientDTO updatedClient);
		Task<DetailsOfClientDTO> DetailsOfClientAsync(int id);
		Task<ListClientsDTO> GetClientsByFilterAsync(int pageSize, int PageNumber, ClientSearchFilter filter);		
		Task<ListClientsDTO> GetAllClientsAsync(int pageSize, int PageNumber);
	}
}
