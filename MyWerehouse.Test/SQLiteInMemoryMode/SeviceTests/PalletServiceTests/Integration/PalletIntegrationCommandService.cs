using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Pallets.Validators;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class PalletIntegrationCommandService : TestBase
	{
		protected readonly PalletService _palletService;		
		public PalletIntegrationCommandService()
		{			
			_palletService = new PalletService(Mediator);
		}
	}
}
