using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
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
			_werehouseDbContext.SaveChanges();
			return client.Id;
		}
		public async Task<int> AddClientAsync(Client client)
		{
			await _werehouseDbContext.Clients.AddAsync(client);
			await _werehouseDbContext.SaveChangesAsync();
			return client.Id;
		}
		public void DeleteClientById(int id)
		{
			var client = _werehouseDbContext.Clients.Find(id);
			if (client != null)
			{
				_werehouseDbContext.Remove(client);
				_werehouseDbContext.SaveChanges();
			}
		}
		public async Task DeleteClientByIdAsync(int id)
		{
			var client = await _werehouseDbContext.Clients.FindAsync(id);
			if (client != null)
			{
				_werehouseDbContext.Remove(client);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void SwitchOffClient(int id)
		{
			var client = _werehouseDbContext.Clients.Find(id);
			if (client != null)
			{
				client.IsDeleted = true;
				_werehouseDbContext.SaveChanges();
			}
		}
		public async Task SwitchOffClientAsync(int id)
		{
			var client = await _werehouseDbContext.Clients.FindAsync(id);
			if (client != null)
			{
				client.IsDeleted = true;
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public Client? GetClientById(int id)
		{
			if (id > 0)
			{
				var client = _werehouseDbContext.Clients
						.Include(c => c.Addresses)
						.Include(c => c.Issues)
						.Include(c => c.Receipts)
						.SingleOrDefault(c => c.Id == id);
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
		public void UpdateClient(Client client)
		{
			_werehouseDbContext.Attach(client);
			if (client.Name != null)
			{
				_werehouseDbContext.Entry(client).Property(nameof(client.Name)).IsModified = true;
			}
			if (client.Email != null)
			{
				_werehouseDbContext.Entry(client).Property(nameof(client.Email)).IsModified = true;
			}
			if (client.Description != null)
			{
				_werehouseDbContext.Entry(client).Property(nameof(client.Description)).IsModified = true;
			}
			var existingAddress = _werehouseDbContext.Addresses
				.Where(ca => ca.ClientId == client.Id)
				.ToList();
			foreach (var address in existingAddress)
			{
				if (!client.Addresses.Any(i => i.Id == address.Id))
				{
					_werehouseDbContext.Addresses.Remove(address);
				}
			}
			if (client.Addresses != null)
			{
				foreach (var address in client.Addresses)
				{
					var existing = existingAddress.First(a => a.Id == address.Id);
					if (existing ==null)
					{
						address.ClientId = client.Id;
						_werehouseDbContext.Addresses.Add(address);
					}
					else
					{
						existing.Country = address.Country;
						existing.City = address.City;
						existing.Region = address.Region;
						existing.Phone = address.Phone;
						existing.PostalCode = address.PostalCode;
						existing.StreetName = address.StreetName;
						existing.StreetNumber = address.StreetNumber;
						_werehouseDbContext.Entry(existing).State = EntityState.Modified;
					}
				}
			}
			_werehouseDbContext.SaveChanges();
		}
		public async Task UpdateClientAsync(Client client)
		{			
			_werehouseDbContext.Attach(client);
			var clientEntry = _werehouseDbContext.Entry(client);
			if (client.Name != null)
			{
				clientEntry.Property(c => c.Name).IsModified = true;
			}
			if (client.Email != null)
			{
				clientEntry.Property(c => c.Email).IsModified = true;
			}
			if (client.Description != null)
			{
				clientEntry.Property(c => c.Description).IsModified = true;
			}
			var existingAddress = _werehouseDbContext.Addresses
				.Where(ca => ca.ClientId == client.Id)
				.ToList();
			foreach (var address in existingAddress)
			{
				if (!client.Addresses.Any(i => i.Id == address.Id))
				{
					_werehouseDbContext.Addresses.Remove(address);
				}
			}
			if (client.Addresses != null)
			{
				foreach (var address in client.Addresses)
				{
					var existing = existingAddress.FirstOrDefault(a => a.Id == address.Id);
					if (existing == null)
					{
						address.ClientId = client.Id;
						_werehouseDbContext.Addresses.Add(address);
					}
					else
					{
						if (existing.Country != address.Country) existing.Country = address.Country;
						if (existing.City != address.City) existing.City = address.City;
						if (existing.Region != address.Region) existing.Region = address.Region;
						if (existing.Phone != address.Phone) existing.Phone = address.Phone;
						if (existing.PostalCode != address.PostalCode) existing.PostalCode = address.PostalCode;
						if (existing.StreetName != address.StreetName) existing.StreetName = address.StreetName;
						if (existing.StreetNumber != address.StreetNumber) existing.StreetNumber = address.StreetNumber;
						_werehouseDbContext.Entry(existing).State = EntityState.Modified;
					}
				}
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		public IQueryable<Client> GetClients(ClientSearchFilter clientFilter)
		{
			var result = _werehouseDbContext.Clients
				.Where(p => p.IsDeleted == false)
				.Include(c => c.Addresses)
				.AsQueryable();
			if (!string.IsNullOrEmpty(clientFilter.Name))
			{
				result = result.Where(c => c.Name != null && c.Name.Contains(clientFilter.Name, StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrEmpty(clientFilter.Email))
			{
				result = result.Where(c => c.Email != null && c.Email.Contains(clientFilter.Email, StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrEmpty(clientFilter.Description))
			{
				result = result.Where(c => c.Description != null && c.Description.Contains(clientFilter.Description, StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrEmpty(clientFilter.FullName))
			{
				result = result.Where(c => c.FullName != null && c.FullName.Contains(clientFilter.FullName, StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrEmpty(clientFilter.Country))
			{
				result = result.Where(c => c.Addresses.Any(a => a.Country != null && a.Country.Contains(clientFilter.Country, StringComparison.OrdinalIgnoreCase)));
			}

			if (!string.IsNullOrEmpty(clientFilter.City))
			{
				result = result.Where(c => c.Addresses.Any(a => a.City != null && a.City.Contains(clientFilter.City, StringComparison.OrdinalIgnoreCase)));
			}

			if (!string.IsNullOrEmpty(clientFilter.Region))
			{
				result = result.Where(c => c.Addresses.Any(a => a.Region != null && a.Region.Contains(clientFilter.Region, StringComparison.OrdinalIgnoreCase)));
			}

			if (clientFilter.Phone != 0)
			{
				result = result.Where(c => c.Addresses.Any(a => a.Phone == clientFilter.Phone));
			}

			if (!string.IsNullOrEmpty(clientFilter.PostalCode))
			{
				result = result.Where(c => c.Addresses.Any(a => a.PostalCode != null && a.PostalCode.Contains(clientFilter.PostalCode, StringComparison.OrdinalIgnoreCase)));
			}

			if (!string.IsNullOrEmpty(clientFilter.StreetName))
			{
				result = result.Where(c => c.Addresses.Any(a => a.StreetName != null && a.StreetName.Contains(clientFilter.StreetName, StringComparison.OrdinalIgnoreCase)));
			}

			if (!string.IsNullOrEmpty(clientFilter.StreetNumber))
			{
				result = result.Where(c => c.Addresses.Any(a => a.StreetNumber != null && a.StreetNumber.Contains(clientFilter.StreetNumber, StringComparison.OrdinalIgnoreCase)));
			}
			// wyszukiwanie po składowych adresu
			return result;
		}
		public IQueryable<Client> GetAllClients()
		{
			return _werehouseDbContext.Clients.Where(p => p.IsDeleted == false);
		}
	}
}
