using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.Inventories.Commands.ChangeQuantity
{
	public record ChangeQuantityCommand(int ProductId, int Quantity) :IRequest;	
}
