using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
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
	ReasonMovement ReasonMovement,
	string UserId,
	PalletStatus PalletStatus,
	IReadOnlyCollection<PalletMovementDetail> Details) : IDomainEvent;
}
