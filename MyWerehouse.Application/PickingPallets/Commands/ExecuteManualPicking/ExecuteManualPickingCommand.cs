using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteManualPicking
{
	public record ExecuteManualPickingCommand(string PalletId, int IssueId, string UserId):IRequest<PickingResult>;	
}
