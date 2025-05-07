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
		Client GetClientById(int id);
		void UpdateClient (Client client);
		void DeleteClientById (int id);
		void SwitchOffClient(int id);
		IQueryable<Client> GetAllClients();
		IQueryable<Client> GetClients (ClientSearchFilter clientFilter);
	}
}
