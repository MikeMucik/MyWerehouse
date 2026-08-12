using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Issuing.Models
{
	public record IssueVerifyResult(bool IsMatching, bool IsConditional, int PreparedQuantity, int OrderedQuantity, DateOnly? BestBefore);	
}
