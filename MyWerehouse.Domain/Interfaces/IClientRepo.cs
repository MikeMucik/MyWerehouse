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
		void DeleteClient(Client client);
		void SwitchOffClient(Client client);		
		Task<Client?> GetClientByIdAsync (int id); 				
		IQueryable<Client> GetAllClients();
		IQueryable<Client> GetClients (ClientSearchFilter clientFilter);
		Task<bool> IsClientExistAsync(int clientId);
	}
}
