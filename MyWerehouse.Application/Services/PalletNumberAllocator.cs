using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Services
{
	public class PalletNumberAllocator(IPalletRepo palletRepo) : IPalletNumberAllocator
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		
		public async Task<IReadOnlyList<string>> ReserveAsync(int count,CancellationToken ct)
		{
			if (count <= 0)	return [];
			var firstNumber = await _palletRepo.ReservePalletNumbersAsync(count);
			return Enumerable.Range(firstNumber, count)
				.Select(number => $"Q{number:D4}")
				.ToList();
		}
	}
}
