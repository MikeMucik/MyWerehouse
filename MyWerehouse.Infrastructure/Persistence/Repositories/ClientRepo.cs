using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class ClientRepo : IClientRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;

		public ClientRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddClient(Client client)
		{
			_werehouseDbContext.Clients.Add(client);
		}
		public void DeleteClient(Client client)
		{
			_werehouseDbContext.Remove(client);
		}
		public void SwitchOffClient(Client client)
		{
			client.IsDeleted = true;
		}
		public async Task<Client?> GetClientByIdAsync(int id, CancellationToken ct)
		{
			if (id > 0)
			{
				var client = await _werehouseDbContext.Clients
						.Include(c => c.Addresses)
						.Include(c => c.Issues)
						.Include(c => c.Receipts)
						.SingleOrDefaultAsync(c => c.Id == id, ct);
				if (client != null)
				{
					if (client.IsDeleted == false)
					{
						return client;
					}
				}
			}
			return null;
		}
		public async Task<Client?> GetClientToEditAsync(int id, CancellationToken ct)
		{
			var client = await _werehouseDbContext.Clients
					.Where(x => x.IsDeleted == false)
					.Include(c => c.Addresses)
					.SingleOrDefaultAsync(c => c.Id == id, ct);
			return client;
		}

		public async Task<bool> IsClientExistAsync(int clientId, CancellationToken ct)
		{
			var client =await _werehouseDbContext.Clients.Where(x => x.IsDeleted == false)
				.FirstOrDefaultAsync(x=>x.Id == clientId, ct);
			if (client != null)
			{ return true; }
			return false;
		}
	}
}
