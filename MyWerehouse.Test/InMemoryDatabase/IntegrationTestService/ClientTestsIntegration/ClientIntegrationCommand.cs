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
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{
	public class ClientIntegrationCommand : CommandTestBase
	{
		//protected readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		protected readonly ClientService _clientService;
		protected readonly IClientRepo _clientRepo;
		protected readonly IReceiptRepo _receiptRepo;
		protected readonly IIssueRepo _issueRepo;
		protected readonly IValidator<AddAddressDTO> _addressValidator; // Zadeklaruj
		protected readonly IValidator<EditAddressDTO> _editAddressValidator; // Zadeklaruj
		protected readonly IValidator<UpdateClientDTO> _updateClientValidator; // Zadeklaruj
		protected readonly IValidator<AddClientDTO> _addClientValidator; // Zadeklaruj
		public ClientIntegrationCommand() : base()
		{
			//_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
			//	.UseInMemoryDatabase("SharedTestDatabase")
			//	.Options;
			_clientRepo = new ClientRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_issueRepo = new IssueRepo(_context);
			_addressValidator = new AddAddressDTOValidation();
			_editAddressValidator = new EditAddressDTOValidation();
			_addClientValidator = new AddClientDTOValidation(_addressValidator);	
			_updateClientValidator = new UpdateClientDTOValidation(_editAddressValidator);
			_clientService = new ClientService(_clientRepo, _mapper, _receiptRepo,_issueRepo, _context,
				_addClientValidator, _updateClientValidator
								   );
		}
	}
}
