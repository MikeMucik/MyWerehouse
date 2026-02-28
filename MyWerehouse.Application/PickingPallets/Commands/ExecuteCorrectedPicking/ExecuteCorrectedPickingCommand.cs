using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteCorrectedPicking
{
	public record ExecuteCorrectedPickingCommand(string PalletId, Guid IssueId, string UserId):IRequest<PickingResult>;	
}
