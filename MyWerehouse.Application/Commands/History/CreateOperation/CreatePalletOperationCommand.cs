using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Commands.History.CreateOperation
{
	public record CreatePalletOperationCommand(string PalletId,
	int DestinationLocationId,
	ReasonMovement ReasonMovement,
	string UserId,
	PalletStatus PalletStatus,
	IEnumerable<PalletMovementDetail>? Details): INotification;
	
}
