using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Receving.Events
{
	public record HistoryReceiptIssueDetailDto( Guid PalletId, string PalletNumber, int LocationId, string LocationSnapShot);
	
}
