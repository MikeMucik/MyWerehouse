using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.CreatePalletOrAddToPallet
{
	public record CreatePalletOrAddToPalletCommand(int IssueId, int ProductId, int Quantity, string UserId, DateOnly? BestBefore, Allocation Allocation, PickingCompletion PickingCompletion) : IRequest<CreatePalletResult>;	
}
