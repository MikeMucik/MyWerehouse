using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Inventories.Events.ChangeStock
{
	public record StockItemChange(int ProductId, int Quantity);
	public record ChangeStockNotification(StockChangeType ChangeType, IEnumerable<StockItemChange> Changes) : INotification;
}
