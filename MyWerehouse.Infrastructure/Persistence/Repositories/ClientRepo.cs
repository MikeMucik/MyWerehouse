using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class ClientRepo : IClientRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;

		public ClientRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}		
		public int AddClient(Client client)
		{
			_werehouseDbContext.Clients.Add(client);			
			return client.Id;
		}		
		public void DeleteClient(Client client)
		{			
				_werehouseDbContext.Remove(client);					
		}
		public void SwitchOffClient(Client client)
		{
				client.IsDeleted = true;			
		}			
		public async Task<Client?> GetClientByIdAsync(int id)
		{
			if (id > 0)
			{
				var client = await _werehouseDbContext.Clients
						.Include(c => c.Addresses)
						.Include(c => c.Issues)
						.Include(c => c.Receipts)
						.SingleOrDefaultAsync(c => c.Id == id);
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
		public IQueryable<Client> GetClients(ClientSearchFilter clientFilter)
		{
			var result = _werehouseDbContext.Clients
				.Where(p => p.IsDeleted == false);

			if (!string.IsNullOrEmpty(clientFilter.Name))
			{
				result = result.Where(c => c.Name != null && c.Name.StartsWith(clientFilter.Name));				
			}

			if (!string.IsNullOrEmpty(clientFilter.Email))
			{
				result = result.Where(c => c.Email != null && c.Email.StartsWith(clientFilter.Email));
			}

			if (!string.IsNullOrEmpty(clientFilter.Description))
			{
				result = result.Where(c => c.Description != null && c.Description.Contains(clientFilter.Description));
			}

			if (!string.IsNullOrEmpty(clientFilter.FullName))
			{
				result = result.Where(c => c.FullName != null && c.FullName.StartsWith(clientFilter.FullName));
			}
			// wyszukiwanie po składowych adresu
			if (!string.IsNullOrEmpty(clientFilter.Country))
			{
				result = result.Where(c => c.Addresses.Any(a => a.Country != null && a.Country.StartsWith(clientFilter.Country)));				
			}

			if (!string.IsNullOrEmpty(clientFilter.City))
			{
				result = result.Where(c => c.Addresses.Any(a => a.City != null && a.City.StartsWith(clientFilter.City)));				
			}

			if (!string.IsNullOrEmpty(clientFilter.Region))
			{
				result = result.Where(c => c.Addresses.Any(a => a.Region != null && a.Region.StartsWith(clientFilter.Region)));				
			}

			if (clientFilter.Phone != 0)
			{
				result = result.Where(c => c.Addresses.Any(a => a.Phone == clientFilter.Phone));
			}

			if (!string.IsNullOrEmpty(clientFilter.PostalCode))
			{
				result = result.Where(c => c.Addresses.Any(a => a.PostalCode != null && a.PostalCode.StartsWith(clientFilter.PostalCode)));
			}

			if (!string.IsNullOrEmpty(clientFilter.StreetName))
			{
				result = result.Where(c => c.Addresses.Any(a => a.StreetName != null && a.StreetName.StartsWith(clientFilter.StreetName)));
			}

			if (!string.IsNullOrEmpty(clientFilter.StreetNumber))
			{
				result = result.Where(c => c.Addresses.Any(a => a.StreetNumber != null && a.StreetNumber.StartsWith(clientFilter.StreetNumber)));
			}
			
			return result;
		}
		public IQueryable<Client> GetAllClients()
		{
			return _werehouseDbContext.Clients.Where(p => p.IsDeleted == false);
		}

		public async Task< bool> IsClientExistAsync(int clientId)
		{
			if (await _werehouseDbContext.Clients.FindAsync(clientId) != null) { return true; } return false;
		}
	}
}
