using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Common.Mapping;
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
		
		public ReceiptIntegratioCommandService()
		{
			_receiptService = new ReceiptService(Mediator);
		}
	}
}
