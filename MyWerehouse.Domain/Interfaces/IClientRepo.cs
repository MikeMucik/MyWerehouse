using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IClientRepo
	{
		void AddClient (Client client);
		void DeleteClient(Client client);
		void SwitchOffClient(Client client);
		Task<Client?> GetClientByIdAsync (int id, CancellationToken ct);
		Task<Client?> GetClientToEditAsync (int id, CancellationToken ct);
		Task<bool> IsClientExistAsync(int clientId, CancellationToken ct);
	}
}
