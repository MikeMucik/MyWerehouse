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
		void DeleteClient (int id);
		void UpdateClient(AddClientDTO updatedClient);
		DetailsOfClientDTO DetailsOfClient(int id);
		ListClientsDTO GetClientsByFilter(int pageSize, int PageNumber,ClientSearchFilter filter);
		ListClientsDTO GetAllClients(int pageSize, int PageNumber);
	}
}
