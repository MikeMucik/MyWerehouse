using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Pallets.PalletDto
{
	public record ProductOnPalletDto(int ProductId, int Quantity, DateOnly? BestBefore);
	
}
