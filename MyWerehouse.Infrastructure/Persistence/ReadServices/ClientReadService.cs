using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFilter;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Infrastructure.Persistence.ReadServices
{
	public class ClientReadService(WerehouseDbContext werehouseDbContext) : IClientReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		
		public Task<PagedResult<ClientDTO>> GetAllClientsAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var clients = _werehouseDbContext.Clients
				.Where(p => p.IsDeleted == false)
				.AsNoTracking()
				.OrderBy(p => p.Id)
				.Select(x => new ClientDTO
				{
					Id = x.Id,
					Name = x.Name,
					FullName = x.FullName,
					Email = x.Email,
					Description = x.Description,
					Addresses = x.Addresses
					.OrderBy(p => p.Id)
					.Select(a => new AddressDTO
					{
						Country = a.Country,
						City = a.City,
						Region = a.Region,
						Phone = a.Phone,
						PostalCode = a.PostalCode,
						StreetName = a.StreetName,
						StreetNumber = a.StreetNumber,
					})
					.ToList()
				});
			var result = clients.ToPagedResultAsync(pageNumber, pageSize, ct); 
			return result;
		}

		public Task<PagedResult<ClientDTO>> GetClientsByFilterAsync(int pageNumber, int pageSize, ClientSearchFilter filter, CancellationToken ct)
		{
			var result = _werehouseDbContext.Clients
				.Where(p => p.IsDeleted == false);

			if (!string.IsNullOrEmpty(filter.Name))
			{
				result = result.Where(c => c.Name != null && c.Name.StartsWith(filter.Name));
			}

			if (!string.IsNullOrEmpty(filter.Email))
			{
				result = result.Where(c => c.Email != null && c.Email.StartsWith(filter.Email));
			}

			if (!string.IsNullOrEmpty(filter.Description))
			{
				result = result.Where(c => c.Description != null && c.Description.Contains(filter.Description));
			}

			if (!string.IsNullOrEmpty(filter.FullName))
			{
				result = result.Where(c => c.FullName != null && c.FullName.StartsWith(filter.FullName));
			}
			// wyszukiwanie po składowych adresu
			if (!string.IsNullOrEmpty(filter.Country))
			{
				result = result.Where(c => c.Addresses.Any(a => a.Country != null && a.Country.StartsWith(filter.Country)));
			}

			if (!string.IsNullOrEmpty(filter.City))
			{
				result = result.Where(c => c.Addresses.Any(a => a.City != null && a.City.StartsWith(filter.City)));
			}

			if (!string.IsNullOrEmpty(filter.Region))
			{
				result = result.Where(c => c.Addresses.Any(a => a.Region != null && a.Region.StartsWith(filter.Region)));
			}

			if (filter.Phone != 0 && filter.Phone != null)
			{
				result = result.Where(c => c.Addresses.Any(a => a.Phone == filter.Phone));
			}

			if (!string.IsNullOrEmpty(filter.PostalCode))
			{
				result = result.Where(c => c.Addresses.Any(a => a.PostalCode != null && a.PostalCode.StartsWith(filter.PostalCode)));
			}

			if (!string.IsNullOrEmpty(filter.StreetName))
			{
				result = result.Where(c => c.Addresses.Any(a => a.StreetName != null && a.StreetName.StartsWith(filter.StreetName)));
			}

			if (!string.IsNullOrEmpty(filter.StreetNumber))
			{
				result = result.Where(c => c.Addresses.Any(a => a.StreetNumber != null && a.StreetNumber.StartsWith(filter.StreetNumber)));
			}
			var resultToShow = result
					.AsNoTracking()
					.OrderBy(p => p.Id)
					.Select(x => new ClientDTO
					{
						Id = x.Id,
						Name = x.Name,
						FullName = x.FullName,
						Email = x.Email,
						Description = x.Description,
						Addresses = x.Addresses
						.OrderBy(p => p.Id)
						.Select(a => new AddressDTO
						{
							Country = a.Country,
							City = a.City,
							Region = a.Region,
							Phone = a.Phone,
							PostalCode = a.PostalCode,
							StreetName = a.StreetName,
							StreetNumber = a.StreetNumber,
						})
						.ToList()
					});
			return resultToShow.ToPagedResultAsync(pageNumber, pageSize, ct);
		}
		public Task<DetailsOfClientDTO?> DetailsOdClientAsync(int id, CancellationToken ct)
		{
			var client = _werehouseDbContext.Clients
				.AsNoTracking()
				.Where(c => c.Id == id)
				.Select(client => new DetailsOfClientDTO
				{
					Id = client.Id,
					Name = client.Name,
					Email = client.Email,
					Description = client.Description,
					Addresses = client.Addresses
					.OrderBy(p => p.Id)
					.Select(address => new AddressDTO {
						Country = address.Country,
						City = address.City,
						Region = address.Region,
						Phone = address.Phone,
						PostalCode = address.PostalCode,
						StreetName = address.StreetName,
						StreetNumber = address.StreetNumber,
						AdditionalEmail = address.AdditionalEmail,
					})
					.ToList(),
					Receipts = client.Receipts
					.OrderBy(p=>p.ReceiptNumber)
					.Select(receipt => new ReceiptSimplyDTO
					{
						ReceiptId = receipt.Id,
						ReceiptNumber = receipt.ReceiptNumber,
						RampNumber = receipt.RampNumber,
						ReceiptDateTime = receipt.ReceiptDateTime,
						ReceiptStatus = receipt.ReceiptStatus,
						PerformedBy = receipt.PerformedBy,
						ClientId = receipt.ClientId
					})
					.ToList(),
					Issues = client.Issues
					.OrderBy(p=>p.IssueNumber)
					.Select(issue => new IssueSimplyDTO
					{
						Id = issue.Id,
						IssueNumber = issue.IssueNumber,
						IssueDateTimeCreate = issue.IssueDateTimeCreate,
						IssueDateTimeSend = issue.IssueDateTimeSend,
						IssueStatus = issue.IssueStatus,
						PerformedBy = issue.PerformedBy,
						ClientId = issue.ClientId
					})
					.ToList()
				})
				.FirstOrDefaultAsync(ct);
			return client;
		}

		public  Task<ClientDTO?> GetClientByIdAsync(int id, CancellationToken ct)
		{
			var client = _werehouseDbContext.Clients
				.AsNoTracking()
				.Where(c => c.Id == id)
				.Select(client => new ClientDTO
				{
					Id = client.Id,
					Name = client.Name,
					Email = client.Email,
					Description = client.Description,
					FullName = client.FullName,
					Addresses = client.Addresses
					.OrderBy(p => p.Id)
					.Select(address => new AddressDTO
					{
						Id = address.Id,
						Country = address.Country,
						City = address.City,
						Region = address.Region,
						Phone = address.Phone,
						PostalCode = address.PostalCode,
						StreetName = address.StreetName,
						StreetNumber = address.StreetNumber,
						AdditionalEmail = address.AdditionalEmail,
					})
					.ToList()					
				})
				.FirstOrDefaultAsync(ct);
			return client;
		}
	}
}
