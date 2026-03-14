using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class ReversePickingRepo : IReversePickingRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public ReversePickingRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddReversePicking(ReversePicking reversePicking)
		{
			 _werehouseDbContext.Add(reversePicking);
		}
		
		public async Task<bool> ExistsForPickingPalletAsync(string palletId)
		{
			 if(await _werehouseDbContext.ReversePickings.FirstOrDefaultAsync(r=>r.SourcePalletId == palletId) != null) return true ; return false;
		}

		

		public async Task<ReversePicking> GetReversePickingAsync(Guid reversePickingId)
		{
			return await _werehouseDbContext.ReversePickings.FirstOrDefaultAsync(r => r.Id == reversePickingId);
		}

		public IQueryable<ReversePicking> GetReversePickings()
		{
			return  _werehouseDbContext.ReversePickings.Where(r=>r.Status != ReversePickingStatus.Archaive || r.Status != ReversePickingStatus.Completed);
		}

		public Task<List<string>> GetPalletsIdsByDate(DateOnly start, DateOnly end)
		{			
			var task = _werehouseDbContext.ReversePickings.Where(r => r.DateMade >= start && r.DateMade <= end);
			var palletIds = task
				.Select(r => r.PickingPalletId)
				.Distinct()
				.ToListAsync();
			return palletIds;
		}
		//public Task<List<ReversePicking>> GetReverseTaskByDate(DateOnly start, DateOnly end)
		//{
		//	return  _werehouseDbContext.Pallets.Where(r => r.DateMade >= start && r.DateMade <= end).ToListAsync();

		//}
	}
}
