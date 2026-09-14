using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Inventories.Events
{
	public record StockItemChange(Guid ProductId, int Quantity);
	public record ChangeStockNotification(IEnumerable<StockItemChange> Changes) : IDomainEvent;
}
