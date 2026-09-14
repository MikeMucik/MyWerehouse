using FluentValidation;
using MediatR;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Application.Services
{
	public class ClientService : IClientService
	{
		private readonly IClientRepo _clientRepo;		
		private readonly IReceiptRepo _receiptRepo;
		private readonly IIssueRepo _issueRepo;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IClientReadService _clientReadService;
		private readonly IValidator<AddClientDTO> _addClientValidator;
		private readonly IValidator<UpdateClientDTO> _updateClientValidator;

		public ClientService(
			IClientRepo clientRepo,			
			IReceiptRepo receiptRepo,
			IIssueRepo issueRepo,
			IUnitOfWork unitOfWork,
			IClientReadService clientReadService,
			IValidator<AddClientDTO> addClientValidator,
			IValidator<UpdateClientDTO> updateClientValidator)
		{
			_clientRepo = clientRepo;			
			_receiptRepo = receiptRepo;
			_issueRepo = issueRepo;
			_unitOfWork = unitOfWork;
			_clientReadService = clientReadService;
			_addClientValidator = addClientValidator;
			_updateClientValidator = updateClientValidator;
		}

		public async Task<AppResult<int>> AddClientAsync(AddClientDTO addClient, CancellationToken ct)
		{
			var validationResult = await _addClientValidator.ValidateAsync(addClient, ct);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var addresses = new List<Address>();
			foreach (var address in addClient.Addresses)
			{
				var item = new Address
				{
					Country = address.Country,
					City = address.City,
					Region = address.Region,
					Phone = address.Phone,
					PostalCode = address.PostalCode,
					StreetName = address.StreetName,
					StreetNumber = address.StreetNumber,
					AdditionalEmail = address.AdditionalEmail,
				};
				addresses.Add(item);
			}
			var client = new Client
			{
				Name = addClient.Name,
				Email = addClient.Email,
				Description = addClient.Description,
				FullName = addClient.FullName,
				Addresses = addresses,
			};
			_clientRepo.AddClient(client);
			await _unitOfWork.SaveChangesAsync(ct);
			var id = client.Id;
			return AppResult<int>.Success(id);
		}
		public async Task<AppResult<Unit>> DeleteClientAsync(int id, CancellationToken ct)
		{
			var client = await _clientRepo.GetClientByIdAsync(id, ct);
			if (client == null) return AppResult<Unit>.Fail($"Client {id} does not exist.");
			if (!await _issueRepo.HasIssueClient(id, ct) && !await _receiptRepo.HasReceiptClient(id, ct))
			{
				_clientRepo.DeleteClient(client);
			}
			else
			{
				_clientRepo.SwitchOffClient(client);
			}
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<ClientDTO>> GetClientByIdAsync(int id, CancellationToken ct)
		{
			var client = await _clientReadService.GetClientByIdAsync(id, ct);
			if (client == null)
			{
				return AppResult<ClientDTO>.Fail($"Client {id} was not found.");
			}
			return AppResult<ClientDTO>.Success(client);
		}
		public async Task<AppResult<Unit>> UpdateClientAsync(int id, UpdateClientDTO updatedClient, CancellationToken ct)
		{
			var existingClient = await _clientRepo.GetClientToEditAsync(id, ct);
			if (existingClient == null) return AppResult<Unit>.Fail("Client was not found.");
			var validationResult = await _updateClientValidator.ValidateAsync(updatedClient, ct);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			existingClient.Email = updatedClient.Email;
			existingClient.Description = updatedClient.Description;
			existingClient.FullName = updatedClient.FullName;
			existingClient.Name = updatedClient.Name;

			var existingMap = existingClient.Addresses.ToDictionary(a => a.Id);
			var incomingItems = updatedClient.Addresses.ToList();

			var incomingIds = incomingItems
				.Where(a => a.Id != 0)
				.Select(a => a.Id)
				.ToHashSet();

			foreach (var incomingItem in incomingItems)
			{
				if (incomingItem.Id == 0)
				{
					existingClient.Addresses.Add(new Address
					{
						Country = incomingItem.Country,
						City = incomingItem.City,
						Region = incomingItem.Region,
						AdditionalEmail = incomingItem.AdditionalEmail,
						Phone = incomingItem.Phone,
						PostalCode = incomingItem.PostalCode,
						StreetName = incomingItem.StreetName,
						StreetNumber = incomingItem.StreetNumber,
					});
					continue;
				}
				//jeśli jest taki adres(po Id) to podmień wartości
				if (!existingMap.TryGetValue(incomingItem.Id, out var address))
				{
					return AppResult<Unit>.Fail($"Address {incomingItem.Id} was not found.");
				}
				address.Country = incomingItem.Country;
				address.City = incomingItem.City;
				address.Region = incomingItem.Region;
				address.AdditionalEmail = incomingItem.AdditionalEmail;
				address.Phone = incomingItem.Phone;
				address.PostalCode = incomingItem.PostalCode;
				address.StreetName = incomingItem.StreetName;
				address.StreetNumber = incomingItem.StreetNumber;
			}
			var addressesToRemove = existingMap.Values
				.Where(address => !incomingIds.Contains(address.Id))
				.ToList();
			foreach (var address in addressesToRemove)
			{
				existingClient.Addresses.Remove(address);
			}

			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value);
		}

		public async Task<AppResult<DetailsOfClientDTO>> DetailsOfClientAsync(int id, CancellationToken ct)
		{
			var client = await _clientReadService.DetailsOdClientAsync(id, ct);
			if (client != null)
			{
				return AppResult<DetailsOfClientDTO>.Success(client);
			}
			else
			{
				return AppResult<DetailsOfClientDTO>.Fail($"Invalid client ID: {id}.");
			}
		}

		public async Task<AppResult<PagedResult<ClientDTO>>> GetClientsByFilterAsync(int pageNumber, int pageSize, ClientSearchFilter filter, CancellationToken ct)
		{
			var result = await _clientReadService.GetClientsByFilterAsync(pageNumber, pageSize, filter, ct);
			return AppResult<PagedResult<ClientDTO>>.Success(result);
		}
		public async Task<AppResult<PagedResult<ClientDTO>>> GetAllClientsAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var result = await _clientReadService.GetAllClientsAsync(pageNumber, pageSize, ct);
			return AppResult<PagedResult<ClientDTO>>.Success(result);
		}
	}
}
