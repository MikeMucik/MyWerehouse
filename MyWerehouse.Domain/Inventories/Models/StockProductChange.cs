using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Inventories.Models
{
	public record StockProductChange(int ProductId, int Quantity);	
}
