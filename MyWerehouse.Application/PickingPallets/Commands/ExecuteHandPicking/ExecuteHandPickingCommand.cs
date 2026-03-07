using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking
{
	public record ExecuteHandPickingCommand(string PalletIdSource,
		Guid IssueId, int Quanitity, string UserId):IRequest<AppResult<Unit>>;	
}
