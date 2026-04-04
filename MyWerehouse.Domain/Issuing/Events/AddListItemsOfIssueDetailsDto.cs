using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Issuing.Events
{
	public record AddListItemsOfIssueDetailsDto(int Id,Guid ProductId, int Quantity, DateOnly BestBedore);
	
}
