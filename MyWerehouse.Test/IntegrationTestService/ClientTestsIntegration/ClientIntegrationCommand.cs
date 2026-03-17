using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ClientTestsIntegration
{
	public class ClientIntegrationCommand : CommandTestBase
	{
		public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly ClientService _clientService;
		public readonly IClientRepo _clientRepo;
		public readonly IReceiptRepo _receiptRepo;
		protected readonly IValidator<AddressDTO> _addressValidator; // Zadeklaruj
		protected readonly IValidator<UpdateClientDTO> _updateClientValidator; // Zadeklaruj
		protected readonly IValidator<ClientDTO> _addClientValidator; // Zadeklaruj
		public ClientIntegrationCommand() : base()
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("SharedTestDatabase")
				.Options;
			_clientRepo = new ClientRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_addressValidator = new AddressDTOValidation();
			_addClientValidator = new AddClientDTOValidation(_addressValidator);	
			_updateClientValidator = new UpdateClientDTOValidation(_addressValidator);
			_clientService = new ClientService(_clientRepo, _mapper, _receiptRepo, _context,
				_addClientValidator, _updateClientValidator
								   );
		}
	}
}
