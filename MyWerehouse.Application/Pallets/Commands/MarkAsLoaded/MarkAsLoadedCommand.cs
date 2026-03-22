using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public record MarkAsLoadedCommand(Guid PalletId, string UserId) : IRequest<AppResult<Unit>>;	
}
