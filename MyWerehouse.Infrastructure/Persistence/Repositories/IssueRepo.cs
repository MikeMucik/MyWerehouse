using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class IssueRepo(WerehouseDbContext werehouseDbContext) : IIssueRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public void AddIssue(Issue issue)
		{
			_werehouseDbContext.Issues.Add(issue);
		}
		public void DeleteIssue(Issue issue)
		{
			_werehouseDbContext.Issues.Remove(issue);
		}
		public async Task<Issue?> GetIssueByIdAsync(Guid id)
		{
			return await _werehouseDbContext.Issues
				.Include(i => i.Pallets)
					.ThenInclude(l=>l.Location)
				.Include(i => i.Pallets)
					.ThenInclude(p=>p.ProductsOnPallet)
				.Include(i=>i.IssueItems)
				.FirstOrDefaultAsync(i => i.Id == id);
		}
		public async Task<List<Issue>> GetIssuesByIdsAsync(List<Guid> ids)
		{
			return await _werehouseDbContext.Issues
				.Where(i => i.IssueStatus != IssueStatus.Archived && ids.Contains(i.Id))
				.ToListAsync();
		}
		public IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter)
		{
			var result = _werehouseDbContext.Issues
				.Where(i => i.IssueStatus != IssueStatus.Archived);//
			if (filter.ClientId > 0)
			{
				result = result.Where(i => i.ClientId == filter.ClientId);
			}
			if (filter.ClientName != null)
			{
				result = result.Where(i => i.Client.Name == filter.ClientName);
			}
			if (filter.ProductId.HasValue)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.ProductId == filter.ProductId)));
			}
			if (filter.ProductName != null)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.Name == filter.ProductName)));
			}
			if (filter.SKU != null)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.SKU == filter.SKU)));
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

		public IQueryable<Pallet> GetPalletsByIssueId(Guid id)
		{
			var list = _werehouseDbContext.Pallets
				.Include(l=>l.Location)
				.AsNoTracking()
				.Where(p => p.IssueId == id);				
			return list;
		}		

		public async Task<int> GetNextNumberOfIssue()
		{
			var number = await _werehouseDbContext.Issues.MaxAsync(x => (int?)x.IssueNumber) ?? 0;
			return number + 1;
		}

		public async Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid id)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x => x.IssueId == id)
				.Select(x => x.VirtualPallet)
				.Where(x => x != null)
				.Distinct()
				.ToListAsync();
		}
	}
}
