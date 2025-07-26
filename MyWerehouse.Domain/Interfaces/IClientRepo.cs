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
		Task<int> AddClientAsync (Client client);		
		Task DeleteClientByIdAsync(int id);
		Task SwitchOffClientAsync(int id);
		Task<Client?> GetClientByIdAsync (int id); 				
		IQueryable<Client> GetAllClients();
		IQueryable<Client> GetClients (ClientSearchFilter clientFilter);
	}
}
