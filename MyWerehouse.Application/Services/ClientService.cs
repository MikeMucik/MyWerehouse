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
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Services
{
	public class ClientService : IClientService
	{
		private readonly IClientRepo _clientRepo;
		private readonly IMapper _mapper;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IValidator<AddClientDTO> _addClientValidator;
		private readonly IValidator<UpdateClientDTO> _updateClientValidator;

		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper,
			IReceiptRepo receiptRepo,
			IValidator<AddClientDTO>? addClientValidator = null,
			IValidator<UpdateClientDTO> updateClientValidator = null)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			_receiptRepo = receiptRepo;
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
		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper,
			IValidator<UpdateClientDTO> updateClientValidator)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			_updateClientValidator = updateClientValidator;
		}

		public int AddClient(AddClientDTO addClient)
		{
			var validationResult = _addClientValidator.Validate(addClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var client = _mapper.Map<Client>(addClient);
			var id = _clientRepo.AddClient(client);
			return id;
		}
		public async Task<int> AddClientAsync(AddClientDTO addClient)
		{
			var validationResult = await _addClientValidator.ValidateAsync(addClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var client = _mapper.Map<Client>(addClient);
			var id = await _clientRepo.AddClientAsync(client);
			return id;
		}
		public void DeleteClient(int id)
		{
			if (_clientRepo.GetClientById(id) != null)
			{
				var filter = new IssueReceiptSearchFilter
				{
					ClientId = id
				};
				var receipt = _receiptRepo.GetReceiptByFilter(filter);
				if (!receipt.Any())
				{
					_clientRepo.DeleteClientById(id);
				}
				else
				{
					_clientRepo.SwitchOffClient(id);
				}
			}
			else { throw new ArgumentException("Nie ma klienta o tym numerze"); }
		}
		public async Task DeleteClientAsync(int id)
		{
			if (await _clientRepo.GetClientByIdAsync(id) != null)
			{
				var filter = new IssueReceiptSearchFilter
				{
					ClientId = id
				};
				var receipt = _receiptRepo.GetReceiptByFilter(filter);
				if (!await receipt.AnyAsync())
				{
					await _clientRepo.DeleteClientByIdAsync(id);
				}
				else
				{
					await _clientRepo.SwitchOffClientAsync(id);
				}
			}
			else { throw new ArgumentException("Nie ma klienta o tym numerze"); }
		}
		public AddClientDTO GetClientToEdit(int id)
		{
			var client = _clientRepo.GetClientById(id);
			if (client == null) throw new ArgumentException($"Brak klienta o numerze {id}");
			var clientDTO = _mapper.Map<AddClientDTO>(client);
			return clientDTO;
		}
		public async Task<AddClientDTO> GetClientToEditAsync(int id)
		{
			var client = await _clientRepo.GetClientByIdAsync(id);
			if (client == null) throw new ArgumentException($"Brak klienta o numerze {id}");
			var clientDTO = _mapper.Map<AddClientDTO>(client);
			return clientDTO;
		}
		public void UpdateClient(UpdateClientDTO updatedClient)
		{
			var existingClient = _clientRepo.GetClientById(updatedClient.Id);
			var validationResult = _updateClientValidator.Validate(updatedClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			//var client = _mapper.Map<Client>(updatedClient);
			_mapper.Map(updatedClient, existingClient);
			CollectionSynchronizer.SynchronizeCollection(
				 existingClient.Addresses,
				 updatedClient.Addresses,
				 a => a.Id, // Klucz dla adresu
				 a => a.Id, // Klucz dla AddressDTO
				 dto =>
				 {
					 var newAddress = _mapper.Map<Address>(dto);
					 newAddress.ClientId = existingClient.Id;
					 return newAddress;
				 },// Jak dodać nowy
				 (dto, entity) => _mapper.Map(dto, entity) // Jak aktualizować
				 );
			_clientRepo.UpdateClient(existingClient);
		}
		public async Task UpdateClientAsync(UpdateClientDTO updatedClient)
		{
			var existingClient = await _clientRepo.GetClientByIdAsync(updatedClient.Id);
			var validationResult = await _updateClientValidator.ValidateAsync(updatedClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			//var client = _mapper.Map<Client>(updatedClient);
			_mapper.Map(updatedClient, existingClient);
			CollectionSynchronizer.SynchronizeCollection(
				 existingClient.Addresses,
				 updatedClient.Addresses,
				 a => a.Id, // Klucz dla adresu
				 a => a.Id, // Klucz dla AddressDTO
				 dto => _mapper.Map<Address>(dto), // Jak dodać nowy
				 (dto, entity) => _mapper.Map(dto, entity) // Jak aktualizować
				 );
			await _clientRepo.UpdateClientAsync(existingClient);
		}
		//public async Task UpdateClientAsync1(AddClientDTO updatedClient)
		//{
		//	var validationResult = _addClientValidator.Validate(updatedClient);//do testów
		//	if (!validationResult.IsValid)
		//	{
		//		throw new ValidationException(validationResult.Errors);
		//	}
		//	var client = _mapper.Map<Client>(updatedClient);
		//	await _clientRepo.UpdateClientAsync(client);
		//}
		public DetailsOfClientDTO DetailsOfClient(int id)
		{
			var client = _clientRepo.GetClientById(id);
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
		public ListClientsDTO GetClientsByFilter(int pageSize, int pageNumber, ClientSearchFilter filter)
		{
			var products = _clientRepo.GetClients(filter)
				.OrderBy(p => p.Id)
				.ProjectTo<AddClientDTO>(_mapper.ConfigurationProvider)
				//.ToList
				;
			var clientsToShow = products
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToList();
			var clientList = new ListClientsDTO()
			{
				AddClients = clientsToShow,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count = products.Count()
			};
			return clientList;
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
		public ListClientsDTO GetAllClients(int pageSize, int PageNumber)
		{
			var products = _clientRepo.GetAllClients()
				.OrderBy(p => p.Id)
				.ProjectTo<AddClientDTO>(_mapper.ConfigurationProvider);
			var clientsToShow = products
				.Skip(pageSize * (PageNumber - 1))
				.Take(pageSize)
				.ToList();
			var clientList = new ListClientsDTO()
			{
				AddClients = clientsToShow,
				PageSize = pageSize,
				CurrentPage = PageNumber,
				Count = products.Count()
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
