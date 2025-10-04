using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class IssueRepo : IIssueRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public IssueRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
			
		public void AddIssue(Issue issue)
		{
			_werehouseDbContext.Issues.Add(issue);			
		}		
		public async Task DeleteIssueAsync(int id)
		{
			var issue = await _werehouseDbContext.Issues.FindAsync(id);
			if (issue != null)
			{
				_werehouseDbContext.Remove(issue);
			}
		}		
			
		public async Task<Issue?> GetIssueByIdAsync(int id)
		{
			return await _werehouseDbContext.Issues
				.Include(i => i.Pallets)
				.FirstOrDefaultAsync(i => i.Id == id);
		}
		
		public async Task<List<Issue>> GetIssuesByIdsAsync(List<int> ids)
		{
			return await _werehouseDbContext.Issues
				.Where(i => i.IssueStatus != IssueStatus.Archived && ids.Contains(i.Id))
				.ToListAsync();
		}
		public IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter)
		{
			var result = _werehouseDbContext.Issues
				.Where(i=>i.IssueStatus != IssueStatus.Archived)				
				;
			if (filter.IssueId > 0)
			{ 
				result = result.Where(i=>i.Id == filter.IssueId);
			}
				if (filter.ClientId > 0)
			{
				result = result.Where(i => i.ClientId == filter.ClientId);
			}
			if (filter.ClientName != null)
			{
				result = result.Where(i => i.Client.Name == filter.ClientName);
			}
			if (filter.ProductId > 0)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.ProductId == filter.ProductId)));
			}
			if (filter.ProductName != null)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.Name == filter.ProductName)));
			}
			if (filter.DateTimeStart != null)
			{
				var start = filter.DateTimeStart;
				var end = filter.DateTimeEnd ?? DateTime.Now;

				result = result.Where(i => i.IssueDateTimeSend >= start && i.IssueDateTimeSend <= end);
			}
			if (filter.UserId != null)
			{
				result = result.Where(i => i.PerformedBy == filter.UserId);
			}
			
			return result;
		}
		public async Task<List<PalletWithLocation>> GetPalletByIssueIdAsync(int id)
		{
			var list = await _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(p => p.IssueId == id)
				.OrderBy(p => p.LocationId)
				.Select(p => new PalletWithLocation
				{
					PalletId = p.Id.ToString(),
					LocationId = p.LocationId,
				})
				.ToListAsync();
			return list;		
		}
		
	}
}
//// TODO: Improve pallet selection logic.
//// Current implementation selects full pallets using FIFO based on BestBefore date.
//// Future improvements might include:
//// - Handling partial pallet picks (splitting pallet content).
//// - Considering pallet location priorities (e.g., pick from closest).
//// - Avoiding reserved or blocked pallets.
//// - Ensuring stock availability and over-issue prevention.
//// - Tracking remaining quantities and multiple product batches.
////
//// Note: This logic is sufficient for MVP/basic release. Ensure test coverage.