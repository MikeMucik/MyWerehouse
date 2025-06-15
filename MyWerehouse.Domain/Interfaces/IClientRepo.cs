using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IClientRepo
	{
		int AddClient (Client client);
		Task<int> AddClientAsync (Client client);
		void DeleteClientById(int id);
		Task DeleteClientByIdAsync(int id);
		void SwitchOffClient(int id);
		Task SwitchOffClientAsync(int id);
		Client? GetClientById(int id);
		Task<Client?> GetClientByIdAsync (int id); 
		void UpdateClient (Client client);
		Task UpdateClientAsync(Client client);		
		IQueryable<Client> GetAllClients();
		IQueryable<Client> GetClients (ClientSearchFilter clientFilter);
	}
}
