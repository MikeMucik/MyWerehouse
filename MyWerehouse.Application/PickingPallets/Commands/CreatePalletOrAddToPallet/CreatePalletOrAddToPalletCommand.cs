using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.PickingPallets.Commands.CreatePalletOrAddToPallet
{
	public record CreatePalletOrAddToPalletCommand(int IssueId, int ProductId, int Quantity, string UserId, DateOnly? BestBefore): IRequest<Unit>;	
}
