using System;
using System.Data;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken ct);
		Task<T> ExecuteInTransactionAsync<T>(
			Func<CancellationToken, Task<T>> operation, IsolationLevel isolationLevel,
			CancellationToken ct);
	}
}
