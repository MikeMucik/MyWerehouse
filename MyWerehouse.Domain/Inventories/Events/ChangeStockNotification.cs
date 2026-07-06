using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Invetories.Models;

namespace MyWerehouse.Domain.Invetories.Events
{
	public record StockItemChange(Guid ProductId, int Quantity);
	public record ChangeStockNotification(IEnumerable<StockItemChange> Changes) : IDomainEvent;
}
