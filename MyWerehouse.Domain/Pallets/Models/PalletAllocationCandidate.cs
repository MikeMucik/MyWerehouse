using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Pallets.Models
{
	public sealed record PalletAllocationCandidate
	(Guid PalletId, int Quantity);
}
