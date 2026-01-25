using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.IssuesServices
{
	public class AddPalletsToIssueByProductService : IAddPalletsToIssueByProductService
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IProductRepo _productRepo;
		public AddPalletsToIssueByProductService(WerehouseDbContext werehouseDbContext,
			IProductRepo productRepo)
		{
			_werehouseDbContext = werehouseDbContext;
			_productRepo = productRepo;
		}
		public Task<IssueResult> AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product)
		{
			throw new NotImplementedException();
		}
	}
}
