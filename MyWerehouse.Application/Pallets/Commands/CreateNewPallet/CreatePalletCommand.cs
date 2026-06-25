using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public record CreatePalletCommand(CreatePalletDTO DTO, int RampNumber, string UserId)
		: IRequest<AppResult<Unit>>;
}
