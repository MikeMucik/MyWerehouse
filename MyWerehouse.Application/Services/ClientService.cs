using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
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
		private readonly IValidator<AddressDTO> _addressValidator;//do testów
		private readonly IValidator<AddClientDTO> _addClientValidator;//do testów

		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper,
			IReceiptRepo receiptRepo,
			IValidator<AddressDTO>? addressValidator = null,
			IValidator<AddClientDTO>? addClientValidator = null)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			_receiptRepo = receiptRepo;
			_addressValidator = addressValidator;
			_addClientValidator = addClientValidator;
		}
		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper,
			//IReceiptRepo receiptRepo,
			IValidator<AddressDTO>? addressValidator = null,
			IValidator<AddClientDTO>? addClientValidator = null)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			//_receiptRepo = receiptRepo;
			_addressValidator = addressValidator;
			_addClientValidator = addClientValidator;
		}
		public ClientService(
			IClientRepo clientRepo,
			IMapper mapper)
		{
			_clientRepo = clientRepo;
			_mapper = mapper;
			
		}

		public int AddClient(AddClientDTO addClient)
		{
			var validationResult = _addClientValidator.Validate(addClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			if (_addressValidator != null)
			{
				foreach (var address in addClient.Addresses) //do testów
				{
					var resultAddress = _addressValidator.Validate(address);
					if (!resultAddress.IsValid)
					{
						throw new ValidationException(resultAddress.Errors);
					}
				}
			}
			var client = _mapper.Map<Client>(addClient);
			var id = _clientRepo.AddClient(client);
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
			else { throw new InvalidDataException("Nie ma klienta o tym numerze"); }
		}

		public void UpdateClient(AddClientDTO updatedClient)
		{
			var validationResult = _addClientValidator.Validate(updatedClient);//do testów
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			if (_addressValidator != null)
			{
				foreach (var address in updatedClient.Addresses) //do testów
				{
					var resultAddress = _addressValidator.Validate(address);
					if (!resultAddress.IsValid)
					{
						throw new ValidationException(resultAddress.Errors);
					}
				}
			}
			var client = _mapper.Map<Client>(updatedClient);
			_clientRepo.UpdateClient(client);
		}

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
				throw new InvalidDataException("Nie ma takiego klienta");
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

		public ListClientsDTO GetAllClients(int pageSize, int PageNumber)
		{
			var products = _clientRepo.GetAllClients()
				.OrderBy(p => p.Id)
				.ProjectTo<AddClientDTO>(_mapper.ConfigurationProvider)
				//.ToList
				;
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
	}
}
