using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Receiving.Models
{
	public sealed record ReceiptPalletUpdate(
	Guid PalletId,
	string? PalletNumber,
	Guid ProductId,
	int Quantity,
	DateTime DateAdded,
	DateOnly? BestBefore);
}
