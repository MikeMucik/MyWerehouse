using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
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

		public async Task AddReversePickingAsync(ReversePicking reversePicking)
		{
			await _werehouseDbContext.AddAsync(reversePicking);			
		}

		public async Task<ReversePicking> GetReversePickingAsync(int reversePickingId)
		{
			return await _werehouseDbContext.ReversePickings.FirstOrDefaultAsync(r => r.Id == reversePickingId);
		}

		public IQueryable<ReversePicking> GetReversePickings()
		{
			return  _werehouseDbContext.ReversePickings.Where(r=>r.Status != ReversePickingStatus.Archaive || r.Status != ReversePickingStatus.Completed);
		}
	}
}
