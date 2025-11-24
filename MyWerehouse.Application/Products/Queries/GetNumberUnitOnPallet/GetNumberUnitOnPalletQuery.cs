using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.Products.Queries.GetNumberUnitOnPallet
{
	public record AssignPallestResult(int FullPallet, int Rest);
	public record GetNumberUnitOnPalletQuery(int ProductId, int AmountUnits): IRequest<AssignPallestResult>;
	
}
