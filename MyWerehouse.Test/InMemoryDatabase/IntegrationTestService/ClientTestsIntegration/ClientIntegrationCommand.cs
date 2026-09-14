using FluentValidation;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Infrastructure.Persistence.ReadServices;
using MyWerehouse.Test.InMemoryDatabase.Common;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{
	public class ClientIntegrationCommand : CommandTestBase
	{
		protected readonly ClientService _clientService;
		protected readonly IClientRepo _clientRepo;
		protected readonly IReceiptRepo _receiptRepo;
		protected readonly IIssueRepo _issueRepo;
		protected readonly IUnitOfWork _unitOfWork;
		protected readonly IClientReadService _clientReadService;
		protected readonly IValidator<AddressDTO> _addressValidator; // Zadeklaruj		
		protected readonly IValidator<UpdateClientDTO> _updateClientValidator; // Zadeklaruj
		protected readonly IValidator<AddClientDTO> _addClientValidator; // Zadeklaruj
		public ClientIntegrationCommand() : base()
		{			
			_clientRepo = new ClientRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_issueRepo = new IssueRepo(_context);
			_addressValidator = new AddressDTOValidation();
			_unitOfWork = new UnitOfWork(_context);
			_clientReadService = new ClientReadService(_context);
			_addClientValidator = new AddClientDTOValidation(_addressValidator);	
			_updateClientValidator = new UpdateClientDTOValidation(_addressValidator);

			_clientService = new ClientService(_clientRepo, _receiptRepo,_issueRepo,_unitOfWork, _clientReadService,
				_addClientValidator, _updateClientValidator
								   );
		}
	}
}
