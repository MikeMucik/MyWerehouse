using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Pallets.Events
{
	public record PalletHistoryNotification(
	Guid PalletId,
	string PalletNumber,
	int SourceLocationId,
	string SourceSnapshot,
	int DestinationLocationId,
	string DestinationSnapshot,
	ReasonForPallet ReasonMovement,
	string UserId,
	PalletStatus PalletStatus,
	IReadOnlyCollection<HistoryPalletDetail> Details) : IDomainEvent;
}
