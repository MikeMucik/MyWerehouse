using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletDto;

namespace MyWerehouse.Domain.Pallets.Events
{
	public record ChangeStatusOfPalletNotification(
	string PalletId,
	int SourceLocationId,
	string SourceSnapshot,
	int DestinationLocationId,
	string DestinationSnapshot,
	ReasonMovement ReasonMovement,
	string UserId,
	PalletStatus PalletStatus,
	IReadOnlyCollection<PalletMovementDetailDto> Details) : IDomainEvent;
}
