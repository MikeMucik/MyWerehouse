using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ReversePickings.Command.CreateTaskToReversePicking
{
	public record CreateTaskToReversePickingCommand(string PalletId, string UserId):IRequest<Unit>;
}
