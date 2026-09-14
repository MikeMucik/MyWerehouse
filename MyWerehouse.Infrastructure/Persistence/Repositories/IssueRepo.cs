using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

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
		public async Task<Issue?> GetIssueByIdAsync(Guid id, CancellationToken ct)
		{
			return await _werehouseDbContext.Issues
				.Include(i => i.Pallets)
					.ThenInclude(l => l.Location)
				.Include(i => i.Pallets)
					.ThenInclude(p => p.ProductsOnPallet)
				.Include(i => i.IssueItems)
				.FirstOrDefaultAsync(i => i.Id == id, ct);
		}
		public async Task<Issue?> GetIssueByIdForModifyAsync(Guid id, CancellationToken ct)
		{
			return await _werehouseDbContext.Issues
				.Include(i => i.Pallets)
					.ThenInclude(l => l.Location)
				.Include(i => i.Pallets)
					.ThenInclude(p => p.ProductsOnPallet)
				.Include(i => i.IssueItems)
				.Include(i=>i.PickingTasks)
				.FirstOrDefaultAsync(i => i.Id == id, ct);
		}
		public Task<List<Issue>> GetIssuesByDates(DateOnly? startDate, DateOnly? endDate, CancellationToken ct)
		{
				var result = _werehouseDbContext.Issues
					.Where(i => i.IssueStatus != IssueStatus.Archived);
			if (startDate != null)
			{
				var start = startDate;
				var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

				result = result.Where(i => i.IssueDateTimeSend >= start && i.IssueDateTimeSend <= end);
			}
			return result.ToListAsync(ct);
		}
		public async Task<List<Issue>> GetIssuesByIdsAsync(List<Guid> ids, CancellationToken ct)
		{
			return await _werehouseDbContext.Issues
				.Where(i => i.IssueStatus != IssueStatus.Archived && ids.Contains(i.Id))
				.ToListAsync(ct);
		}
		public async Task<int> GetNextNumberOfIssue(CancellationToken ct)
		{
			var number = await _werehouseDbContext.Issues.MaxAsync(x => (int?)x.IssueNumber, ct) ?? 0;
			return number + 1;
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid id, CancellationToken ct)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x => x.IssueId == id)
				.Where(x => x.VirtualPallet != null)
				.Include(x => x.VirtualPallet!.Pallet)
				.Include(x => x.VirtualPallet!.PickingTasks)
				.Select(x => x.VirtualPallet!)
				.Distinct()
				.ToListAsync(ct);
		}
		public Task<bool> HasIssueClient(int clientId, CancellationToken ct)
		{
			return _werehouseDbContext.Issues
				.AnyAsync(c => c.ClientId == clientId, ct);
		}
	}
}
