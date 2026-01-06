using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Domain.Clients.Filters;

namespace MyWerehouse.Application.Services
{
	public class ClientService : IClientService
	{
		private readonly IClientRepo _clientRepo;
		private readonly IMapper _mapper;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IValidator<AddClientDTO> _addClientValidator;
		private readonly IValidator<UpdateClientDTO> _updateClientValidator;
		private readonly WerehouseDbContext _werehouseDbContext;
		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper,
			IReceiptRepo receiptRepo,
			WerehouseDbContext werehouseDbContext,
			IValidator<AddClientDTO>? addClientValidator = null,
			IValidator<UpdateClientDTO>? updateClientValidator = null)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			_receiptRepo = receiptRepo;
			_werehouseDbContext = werehouseDbContext;
			_addClientValidator = addClientValidator;
			_updateClientValidator = updateClientValidator;
		}
		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper,
			IValidator<AddClientDTO>? addClientValidator = null,
			IValidator<UpdateClientDTO>? updateClientValidator = null)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			_addClientValidator = addClientValidator;
			_updateClientValidator = updateClientValidator;
		}
		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
		}
		
		public async Task<int> AddClientAsync(AddClientDTO addClient)
		{
			var validationResult = await _addClientValidator.ValidateAsync(addClient);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var client = _mapper.Map<Client>(addClient);
			var id = _clientRepo.AddClient(client);
			await _werehouseDbContext.SaveChangesAsync();
			return id;
		}
		public async Task DeleteClientAsync(int id)
		{			
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = id
			};
			var client = await _clientRepo.GetClientByIdAsync(id) ?? throw new DomainException($"Klient o numerze {id} nie istnieje.");
			var receipt = _receiptRepo.GetReceiptByFilter(filter);
			if (!await receipt.AnyAsync())
			{
				_clientRepo.DeleteClient(client);
			}
			else
			{
				_clientRepo.SwitchOffClient(client);
				//await _clientRepo.SwitchOffClientAsync(id);
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<AddClientDTO> GetClientToEditAsync(int id)
		{
			var client = await _clientRepo.GetClientByIdAsync(id);
			if (client == null) throw new ArgumentException($"Brak klienta o numerze {id}");
			var clientDTO = _mapper.Map<AddClientDTO>(client);
			return clientDTO;
		}
		public async Task UpdateClientAsync(UpdateClientDTO updatedClient)
		{
			var existingClient = await _clientRepo.GetClientByIdAsync(updatedClient.Id);
			var validationResult = await _updateClientValidator.ValidateAsync(updatedClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			_mapper.Map(updatedClient, existingClient);
			CollectionSynchronizer.SynchronizeCollection(
				 existingClient.Addresses,
				 updatedClient.Addresses,
				 a => a.Id, // Klucz dla adresu
				 a => a.Id, // Klucz dla AddressDTO
				 dto => _mapper.Map<Address>(dto), // Jak dodać nowy
				 (dto, entity) => _mapper.Map(dto, entity) // Jak aktualizować
				 );
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<DetailsOfClientDTO> DetailsOfClientAsync(int id)
		{
			var client = await _clientRepo.GetClientByIdAsync(id);
			if (client != null)
			{
				var clientToShow = _mapper.Map<DetailsOfClientDTO>(client);
				return clientToShow;
			}
			else
			{
				throw new ArgumentException("Nie ma takiego klienta");
			}
		}
		public async Task<ListClientsDTO> GetClientsByFilterAsync(int pageSize, int pageNumber, ClientSearchFilter filter)
		{
			var products = _clientRepo.GetClients(filter)
				.OrderBy(p => p.Id)
				.ProjectTo<AddClientDTO>(_mapper.ConfigurationProvider);
			var clientsToShow = await products
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync();
			var clientList = new ListClientsDTO()
			{
				AddClients = clientsToShow,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count = await products.CountAsync()
			};
			return clientList;
		}
		public async Task<ListClientsDTO> GetAllClientsAsync(int pageSize, int PageNumber)
		{
			var products = _clientRepo.GetAllClients()
				.OrderBy(p => p.Id)
				.ProjectTo<AddClientDTO>(_mapper.ConfigurationProvider);
			var clientsToShow = await products
				.Skip(pageSize * (PageNumber - 1))
				.Take(pageSize)
				.ToListAsync();
			var clientList = new ListClientsDTO()
			{
				AddClients = clientsToShow,
				PageSize = pageSize,
				CurrentPage = PageNumber,
				Count = await products.CountAsync()
			};
			return clientList;
		}
	}
}
