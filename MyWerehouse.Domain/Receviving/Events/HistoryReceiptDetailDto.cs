using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Receviving.Events
{
	public record HistoryReceiptDetailDto(string PalletId, int LocationId, Location Location);
	
}
