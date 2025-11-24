using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Validators;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using static MyWerehouse.Application.Issues.DTOs.CreateIssueDTO;
using static MyWerehouse.Application.Issues.DTOs.UpdateIssueDTO;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueIntegrationCommandService : TestBase
	{		
		protected readonly IssueService _issueService;
		protected readonly IMapper _mapper;

		protected readonly IIssueRepo _issueRepo;
		protected readonly IIssueItemRepo _issueItemRepo;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IAllocationRepo _allocationRepo;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		
		public IssueIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();			
		
			_palletRepo = new PalletRepo(DbContext);
			_issueRepo = new IssueRepo(DbContext);
			_allocationRepo = new AllocationRepo(DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			
			_issueItemRepo = new IssueItemRepo(DbContext);			
			_issueService = new IssueService(
				Mediator,
				_issueRepo,
				_mapper,
				DbContext,			
				_palletRepo,
				_allocationRepo,
				_pickingPalletRepo,
				_issueItemRepo);
		}
	}
}
