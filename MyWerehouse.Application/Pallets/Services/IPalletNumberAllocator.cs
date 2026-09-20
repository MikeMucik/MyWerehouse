using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Pallets.Services
{
	public interface IPalletNumberAllocator
	{
		Task<IReadOnlyList<string>> ReserveAsync(int count, CancellationToken ct);
	}
}
