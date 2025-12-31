using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Application.Receipts.Validators;
using static MyWerehouse.Application.Receipts.DTOs.CreateReceiptPlanDTO;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptIntegratioCommandService : TestBase
	{
		protected readonly ReceiptService _receiptService;
		//protected readonly IMapper _mapper;
		//protected readonly IReceiptRepo _receiptRepo;
		//protected readonly IPalletRepo _palletRepo;			
		
		public ReceiptIntegratioCommandService()
		{
			//var MapperConfig = new MapperConfiguration(cfg =>
			//{
			//	cfg.AddProfile<MappingProfile>();
			//});
			//_mapper = MapperConfig.CreateMapper();			
						
			//_receiptRepo = new ReceiptRepo(DbContext);
			//_palletRepo = new PalletRepo(DbContext);
			_receiptService = new ReceiptService(Mediator
				//,
				//_receiptRepo,
				//_mapper,
				//DbContext,
				//_palletRepo
				);
		}
	}
}
