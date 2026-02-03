using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.ReversePickingServiceTests.Integration
{
	public class ReverseIntegrationCommandService : TestBase
	{
		protected readonly ReversePickingService _reversePickingService;
		//protected readonly IReversePickingRepo _reversePickingRepo;
		//protected readonly IPickingTaskRepo _pickingTaskRepo;
		//protected readonly IPalletRepo _palletRepo;
		//protected readonly IProductRepo _productRepo;		
		////protected readonly IMapper _mapper;
		//protected readonly IEventCollector _eventCollector;

		public ReverseIntegrationCommandService()
		{
			//var MapperConfig = new MapperConfiguration(cfg =>
			//{
			//	cfg.AddProfile<MappingProfile>();
			//});
			//_pickingTaskRepo = new PickingTaskRepo(DbContext);
			//_reversePickingRepo = new ReversePickingRepo(DbContext);
			//_palletRepo = new PalletRepo(DbContext);
			//_productRepo = new ProductRepo(DbContext);
			//_eventCollector = new EventCollector();


			_reversePickingService = new ReversePickingService( Mediator);
		}
	}
}
