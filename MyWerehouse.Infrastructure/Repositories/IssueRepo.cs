using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			_werehouseDbContext.Issues
				.Add(issue);
			_werehouseDbContext.SaveChanges();
		}
		public async Task AddIssueAsync(Issue issue)
		{
			_werehouseDbContext.Issues
			   .Add(issue);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//public void AddPalletsToIssue(int issueId, int productId, int quantityRequested, DateOnly bestBefore)
		//{
		//	var issue = _werehouseDbContext.Issues
		//		.Include(p=>p.Pallets)
		//		.FirstOrDefault(p=>p.Id == issueId);

		//	var availablePallets = _werehouseDbContext.Pallets
		//		.Include(p => p.ProductsOnPallet)
		//		.Where(p => p.ProductsOnPallet.Any(pp => pp.ProductId == productId))
		//		.Where(p => p.IssueId == null)
		//		.AsQueryable();

		//	if (bestBefore != null)
		//	{
		//		availablePallets = availablePallets
		//			.Where(p => p.ProductsOnPallet.Any(pp => pp.BestBefore >= bestBefore));
		//	}
		//	var selectedPallets = new List<Pallet>();
		//	int totalQuantity = 0;
		//	foreach (var pallet in availablePallets.OrderBy(p=>p.ProductsOnPallet.Min(pp=>pp.DateAdded)))
		//	{
		//		var quantittOnPallet = pallet.ProductsOnPallet
		//			.Where(pp => pp.ProductId == productId)
		//			.Sum(pp => pp.Quantity);

		//		selectedPallets.Add(pallet);
		//		totalQuantity += quantittOnPallet;

		//		if(totalQuantity >= quantityRequested) 
		//			break;
		//	}
		//	if (totalQuantity < quantityRequested)
		//		throw new InvalidOperationException("Brak wystarczającej ilości do zamówienia");
		//	foreach (var pallet in selectedPallets)
		//	{
		//		pallet.IssueId = issue.Id;
		//		issue.Pallets.Add (pallet);
		//	}
		//	_werehouseDbContext.SaveChanges();
		//}
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

		public void DeleteIssue(int id)
		{
			var issue = _werehouseDbContext.Issues.Find(id);
			if (issue != null)
			{
				_werehouseDbContext.Remove(issue);
				_werehouseDbContext.SaveChanges();
			}
		}
		public async Task DeleteIssueAsync(int id)
		{
			var issue = await _werehouseDbContext.Issues.FindAsync(id);
			if (issue != null)
			{
				_werehouseDbContext.Remove(issue);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void UpdateIssue(Issue issue)
		{
			_werehouseDbContext.Attach(issue);
			var existingPallets = _werehouseDbContext.Pallets
				.Where(p => p.IssueId == issue.Id)
				.ToList();
			_werehouseDbContext.Attach(issue);
			if (issue.IssueDateTime != DateTime.MinValue)
			{
				_werehouseDbContext.Entry(issue).Property(nameof(issue.IssueDateTime)).IsModified = true;
			}
			if (issue.PerformedBy != null)
			{
				_werehouseDbContext.Entry(issue).Property(nameof(issue.PerformedBy)).IsModified = true;
			}
			_werehouseDbContext.SaveChanges();
		}
		public async Task UpdateIssueAsync(Issue issue)
		{
			_werehouseDbContext.Attach(issue);
			if (issue.IssueDateTime != DateTime.MinValue)
			{
				_werehouseDbContext.Entry(issue).Property(nameof(issue.IssueDateTime)).IsModified = true;
			}
			if (issue.PerformedBy != null)
			{
				_werehouseDbContext.Entry(issue).Property(nameof(issue.PerformedBy)).IsModified = true;
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		public Issue? GetIssueById(int id)
		{
			return _werehouseDbContext.Issues
				.Include(i => i.Pallets)
				.SingleOrDefault(i => i.Id == id);
		}
		public async Task<Issue?> GetIssueByIdAsync(int id)
		{
			return await _werehouseDbContext.Issues
				.Include(i => i.Pallets)
				.SingleOrDefaultAsync(i => i.Id == id);
		}
		public IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter)
		{
			var result = _werehouseDbContext.Issues
				.Include(i => i.Client)
				.Include(i => i.Pallets)
					.ThenInclude(ip => ip.ProductsOnPallet)
						.ThenInclude(ipp => ipp.Product)
				.AsQueryable();
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

				result = result.Where(i => i.IssueDateTime >= start && i.IssueDateTime <= end);
			}
			if (filter.UserId != null)
			{
				result = result.Where(i => i.PerformedBy == filter.UserId);
			}
			return result;
		}
		//public IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly minBestBeforeDate)
		//{
		//	var pallets = _werehouseDbContext.Pallets
		//		.Include(p=>p.ProductsOnPallet)
		//		.Where(p => p.ProductsOnPallet.Any(pp => pp.ProductId
		//		== productId && pp.BestBefore >= minBestBeforeDate && pp.Pallet.Status == PalletStatus.Available))
		//		.OrderBy(p => p.ProductsOnPallet
		//			.Where(pp => pp.ProductId == productId)
		//			.Min(pp => pp.BestBefore))
		//		.ThenBy(p => p.LocationId);
		//	return pallets;
		//}
		//public List<Pallet> SelectPalletsForIssue(IQueryable<Pallet> pallets, int quantity)
		//{
		//	var result = new List<Pallet>();
		//	int collected = 0;
		//	foreach (var pallet in pallets)
		//	{
		//		var productOnPallet = pallet.ProductsOnPallet.FirstOrDefault();
		//		if (productOnPallet == null)
		//			continue;
		//		collected += productOnPallet.Quantity;
		//		result.Add(pallet);
		//		if (collected >= quantity)
		//		{
		//			if (collected > quantity)
		//			{
		//				pallet.Status = PalletStatus.ToPicking;
		//			}
		//			break;
		//		}
		//	}
		//	return result;
		//}
		//public async Task<List<Pallet>> SelectPalletsForIssueAsync(IQueryable<Pallet> pallets, int quantity)
		//{
		//	var palletsToIssue = await pallets
		//		.Include(p => p.ProductsOnPallet)
		//		.ToListAsync();
		//	var result = new List<Pallet>();
		//	int collected = 0;

		//	foreach (var pallet in palletsToIssue)
		//	{
		//		var productOnPallet = pallet.ProductsOnPallet.FirstOrDefault();
		//		if (productOnPallet == null)
		//			continue;
		//		collected += productOnPallet.Quantity;
		//		result.Add(pallet);
		//		if (collected >= quantity)
		//		{
		//			if (collected > quantity)
		//			{
		//				pallet.Status = PalletStatus.ToPicking;
		//			}
		//			break;
		//		}
		//	}
		//	return result;
		//}
	}
}
