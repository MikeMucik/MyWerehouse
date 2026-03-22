using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Picking.Models
{
	public record CreatePalletResult(bool NewPalletCreated, Guid PalletId);	
}
