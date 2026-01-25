using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.ReversePickings.Command.CreateTaskToReversePicking
{
	public record CreateTaskToReversePickingCommand(string PalletId, string UserId):IRequest<Unit>;
}
