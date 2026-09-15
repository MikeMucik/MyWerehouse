using System.Data;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Infrastructure.Common
{
	public class UnitOfWork(WerehouseDbContext werehouseDbContext) : IUnitOfWork
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		//for CreateIssue & ModifyIssue & CreateReceipt 
		public Task<T> ExecuteInTransactionAsync<T>(
			Func< CancellationToken, Task<T>> operation,
			IsolationLevel isolationLevel,
			CancellationToken ct)
		{
			var strategy = 
				_werehouseDbContext.Database.CreateExecutionStrategy();

			return strategy.ExecuteAsync(async () =>
			{
				_werehouseDbContext.ChangeTracker.Clear();

				await using var transaction = 
				await _werehouseDbContext.Database
				.BeginTransactionAsync(isolationLevel, ct);

				var result = await operation(ct);

				await transaction.CommitAsync(ct);

				return result;
			});
		}
		public async Task<int> SaveChangesAsync(CancellationToken ct)
		{
			return await _werehouseDbContext.SaveChangesAsync(ct);
		}
	}
}
