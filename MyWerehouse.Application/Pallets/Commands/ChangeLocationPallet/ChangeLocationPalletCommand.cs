using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet
{
	public record ChangeLocationPalletCommand(string PalletId, int DestinationLocationId, string UserId, bool Force = false):IRequest<ChangeLocationResults>;
	
}
