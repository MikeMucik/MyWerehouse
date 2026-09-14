using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
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
