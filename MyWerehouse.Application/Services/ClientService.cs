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
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Infrastructure;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Application.Common.Utils;
using MyWerehouse.Application.Common.Results;
using MediatR;

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

		public async Task<AppResult<int>> AddClientAsync(AddClientDTO addClient)
		{
			var validationResult = await _addClientValidator.ValidateAsync(addClient);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var client = _mapper.Map<Client>(addClient);
			var id = _clientRepo.AddClient(client);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<int>.Success(id);
		}
		public async Task<AppResult<Unit>> DeleteClientAsync(int id)
		{
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = id
			};
			var client = await _clientRepo.GetClientByIdAsync(id);// ?? throw new DomainException($"Klient o numerze {id} nie istnieje.");
			if (client == null) return AppResult<Unit>.Fail($"Klient o numerze {id} nie istnieje.", ErrorType.NotFound);
			var receipt = _receiptRepo.GetReceiptByFilter(filter);
			if (!await receipt.AnyAsync())
			{
				_clientRepo.DeleteClient(client);
			}
			else
			{
				_clientRepo.SwitchOffClient(client);
			}
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<AddClientDTO>> GetClientToEditAsync(int id)
		{
			var client = await _clientRepo.GetClientByIdAsync(id);
			if (client == null) throw new ArgumentException($"Brak klienta o numerze {id}");
			var clientDTO = _mapper.Map<AddClientDTO>(client);
			return AppResult<AddClientDTO>.Success( clientDTO);
		}
		public async Task<AppResult<Unit>> UpdateClientAsync(UpdateClientDTO updatedClient)
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
				 (dto, entity) => _mapper.Map(dto, entity), // Jak aktualizować
				entity => existingClient.Addresses.Remove(entity)//Jak usuwać adresy
				 );
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<DetailsOfClientDTO>> DetailsOfClientAsync(int id)
		{
			var client = await _clientRepo.GetClientByIdAsync(id);
			if (client != null)
			{
				var clientToShow = _mapper.Map<DetailsOfClientDTO>(client);
				return AppResult<DetailsOfClientDTO>.Success( clientToShow);
			}
			else
			{
				return AppResult<DetailsOfClientDTO>.Fail($"Nieprawidłowy numer client {id}.", ErrorType.NotFound);
				//throw new NotFoundClientException(id);
			}
		}
		public async Task<AppResult<ListClientsDTO>> GetClientsByFilterAsync(int pageSize, int pageNumber, ClientSearchFilter filter)
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
			return AppResult<ListClientsDTO>.Success( clientList);
		}
		public async Task<AppResult<ListClientsDTO>> GetAllClientsAsync(int pageSize, int PageNumber)
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
			return AppResult<ListClientsDTO>.Success(clientList);
		}
	}
}
