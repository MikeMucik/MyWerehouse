using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.ClientTestsIntegration
{
	public class ClientIntegrationCommand : CommandTestBase
	{
		public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly ClientService _clientService;
		public readonly IMapper _mapper;
		public readonly IClientRepo _clientRepo;
		public readonly IReceiptRepo _receiptRepo;
		protected readonly IValidator<AddressDTO> _addressValidator; // Zadeklaruj
		protected readonly IValidator<UpdateClientDTO> _updateClientValidator; // Zadeklaruj
		protected readonly IValidator<AddClientDTO> _addClientValidator; // Zadeklaruj
		public ClientIntegrationCommand() : base()
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("SharedTestDatabase")
				.Options;
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();
			_clientRepo = new ClientRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_addressValidator = new AddressDTOValidation();
			_addClientValidator = new AddClientDTOValidation(_addressValidator);	
			_updateClientValidator = new UpdateClientDTOValidation(_addressValidator);
			_clientService = new ClientService(_clientRepo, _mapper, _receiptRepo, _context,
				_addClientValidator, _updateClientValidator);
		}
	}
}
