using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ClientTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ClientIntegrationView : CommandTestBase
	{
		public readonly ClientService _clientService;
		public readonly ClientRepo _clientRepo;
		public ClientIntegrationView(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_clientRepo = new ClientRepo(_context);			
			_clientService = new ClientService(_clientRepo, _mapper);
		}
	}
}
