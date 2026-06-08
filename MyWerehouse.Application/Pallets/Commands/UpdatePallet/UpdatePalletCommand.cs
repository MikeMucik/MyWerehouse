using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pallets.Commands.UpdatePallet
{
	public record UpdatePalletCommand(Guid Id, EditPalletDTO UpdatingPallet) : IRequest<AppResult<Unit>>;
}