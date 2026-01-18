using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Products.Queries.GetNumberPalletsAndRest
{
	//public record AssignPallestResult(int FullPallet, int Rest);
	public record GetNumberPalletsAndRestQuery(int ProductId, int AmountUnits): IRequest<AssignPallestResult>;	
}
