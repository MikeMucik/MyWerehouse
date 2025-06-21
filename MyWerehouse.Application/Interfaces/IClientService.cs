using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IClientService
	{
		int AddClient(AddClientDTO addClient);
		Task<int> AddClientAsync(AddClientDTO addClient);
		void DeleteClient(int id);
		Task DeleteClientAsync(int id);
		AddClientDTO GetClientToEdit(int id);
		Task<AddClientDTO> GetClientToEditAsync(int id);
		void UpdateClient(AddClientDTO updatedClient);
		Task UpdateClientAsync(AddClientDTO updatedClient);
		DetailsOfClientDTO DetailsOfClient(int id);
		Task<DetailsOfClientDTO> DetailsOfClientAsync(int id);
		ListClientsDTO GetClientsByFilter(int pageSize, int PageNumber, ClientSearchFilter filter);
		Task<ListClientsDTO> GetClientsByFilterAsync(int pageSize, int PageNumber, ClientSearchFilter filter);
		ListClientsDTO GetAllClients(int pageSize, int PageNumber);
		Task<ListClientsDTO> GetAllClientsAsync(int pageSize, int PageNumber);
	}
}
