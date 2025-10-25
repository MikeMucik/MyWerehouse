using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.PalletTestsIntegration
{
	[Collection("QuerryCollection")]
	public class PalletIntegrationView : CommandTestBase
	{
		public readonly PalletService _palletService;
		public readonly PalletRepo _palletRepo;
		public readonly IMapper _mapper;
		public PalletIntegrationView(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = mapperConfig.CreateMapper();
			_palletRepo = new PalletRepo(_context);

			_palletService = new PalletService(_palletRepo, _mapper);
		}
	}
}
