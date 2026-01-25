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
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using static MyWerehouse.Application.Issues.DTOs.CreateIssueDTO;
using static MyWerehouse.Application.Issues.DTOs.UpdateIssueDTO;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueIntegrationCommandService : TestBase
	{		
		protected readonly IssueService _issueService;		
		public IssueIntegrationCommandService()
		{
			_issueService = new IssueService(Mediator);
		}
	}
}
