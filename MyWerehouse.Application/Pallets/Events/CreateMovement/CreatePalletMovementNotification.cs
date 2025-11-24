using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Pallets.Events.CreateMovement
{
	public record CreatePalletMovementNotification(string PalletId,int SourceLocationId,
	int DestinationLocationId,
	ReasonMovement ReasonMovement,
	string UserId,
	PalletStatus PalletStatus,
	IEnumerable<PalletMovementDetail>? Details): INotification;

}
