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
		public readonly IValidator<AddressDTO> _addressValidator;
		public readonly IValidator<AddClientDTO> _clientValidator;
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
			var _clientRepo = new ClientRepo(_context);
			var _receiptRepo = new ReceiptRepo(_context);
			_addressValidator = new AddressDTOValidation();
			_clientValidator = new AddClientDTOValidation();			
			_clientService = new ClientService(_clientRepo, _mapper, _receiptRepo);
			_clientService = new ClientService(_clientRepo, _mapper, _receiptRepo, _addressValidator, _clientValidator);

		}

	}
}
